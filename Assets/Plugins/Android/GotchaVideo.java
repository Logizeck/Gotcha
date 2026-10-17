package com.gotcha.video;

import android.app.Activity;
import android.content.ContentResolver;
import android.content.ContentValues;
import android.content.Intent;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.media.Image;
import android.media.MediaCodec;
import android.media.MediaCodecInfo;
import android.media.MediaFormat;
import android.media.MediaMuxer;
import android.net.Uri;
import android.os.Build;
import android.os.StrictMode;
import android.provider.MediaStore;

import com.unity3d.player.UnityPlayer;

import java.io.File;
import java.io.FileInputStream;
import java.io.InputStream;
import java.io.OutputStream;
import java.nio.ByteBuffer;
import java.util.ArrayList;
import java.util.Collections;
import java.util.Comparator;
import java.util.List;

/**
 * Codifica in MP4 (H.264 + audio AAC) i fotogrammi e il suono catturati dal gioco e li condivide.
 * Uso da Unity (C#): begin(path, w, h, fps) → addJpeg(bytes) per ogni fotogramma → addAudio(pcm, rate, canali)
 * (facoltativo) → finish() → share(...).
 * I fotogrammi arrivano come JPEG già dritti (li comprime Unity), qui vengono convertiti in YUV 4:2:0.
 * L'audio è PCM 16 bit little-endian interlacciato. I campioni codificati restano in memoria (pochi MB per
 * una clip di 14 secondi) e vengono scritti nel file solo alla fine, quando entrambe le tracce sono pronte.
 */
public class GotchaVideo {
    private static final String MIME = "video/avc";
    private static final String AUDIO_MIME = "audio/mp4a-latm";

    /** Un pezzo già codificato, in attesa di essere scritto nel file. */
    private static final class Sample {
        final byte[] data;
        final long pts;
        final int flags;
        final boolean audio;
        Sample(byte[] data, long pts, int flags, boolean audio) { this.data = data; this.pts = pts; this.flags = flags; this.audio = audio; }
    }

    private final MediaCodec codec;
    private final String path;
    private final MediaCodec.BufferInfo info = new MediaCodec.BufferInfo();
    private final int width, height, fps;
    private final List<Sample> samples = new ArrayList<>();
    private MediaFormat videoFormat, audioFormat;
    private long frameIndex;
    private int[] argb;

    private GotchaVideo(String path, int width, int height, int fps) throws Exception {
        this.path = path;
        this.width = width;
        this.height = height;
        this.fps = fps;
        MediaFormat format = MediaFormat.createVideoFormat(MIME, width, height);
        format.setInteger(MediaFormat.KEY_COLOR_FORMAT, MediaCodecInfo.CodecCapabilities.COLOR_FormatYUV420Flexible);
        format.setInteger(MediaFormat.KEY_BIT_RATE, 2_500_000);
        format.setInteger(MediaFormat.KEY_FRAME_RATE, fps);
        format.setInteger(MediaFormat.KEY_I_FRAME_INTERVAL, 1);
        codec = MediaCodec.createEncoderByType(MIME);
        codec.configure(format, null, null, MediaCodec.CONFIGURE_FLAG_ENCODE);
        codec.start();
    }

    /** Inizia un nuovo video. Larghezza e altezza devono essere multipli di 16. */
    public static GotchaVideo begin(String path, int width, int height, int fps) throws Exception {
        return new GotchaVideo(path, width, height, fps);
    }

    /** Aggiunge un fotogramma (JPEG già orientato correttamente). */
    public void addJpeg(byte[] jpeg) {
        Bitmap bmp = BitmapFactory.decodeByteArray(jpeg, 0, jpeg.length);
        if (bmp == null) return;
        if (bmp.getWidth() != width || bmp.getHeight() != height) {
            Bitmap scaled = Bitmap.createScaledBitmap(bmp, width, height, true);
            bmp.recycle();
            bmp = scaled;
        }
        if (argb == null) argb = new int[width * height];
        bmp.getPixels(argb, 0, width, 0, 0, width, height);
        bmp.recycle();

        int index = codec.dequeueInputBuffer(20_000);
        if (index >= 0) {
            Image image = codec.getInputImage(index);
            if (image != null) fillYuv(image);
            long pts = frameIndex * 1_000_000L / fps;
            codec.queueInputBuffer(index, 0, width * height * 3 / 2, pts, 0);
            frameIndex++;
        }
        drainVideo(false);
    }

    /**
     * Aggiunge la colonna sonora (PCM 16 bit little-endian, canali interlacciati, 1 o 2 canali).
     * Va chiamato una volta sola, prima di finish(). Se l'encoder AAC non c'è, il video resta muto.
     */
    public void addAudio(byte[] pcm, int sampleRate, int channels) {
        MediaCodec aac = null;
        try {
            MediaFormat format = MediaFormat.createAudioFormat(AUDIO_MIME, sampleRate, channels);
            format.setInteger(MediaFormat.KEY_AAC_PROFILE, MediaCodecInfo.CodecProfileLevel.AACObjectLC);
            format.setInteger(MediaFormat.KEY_BIT_RATE, 128_000);
            format.setInteger(MediaFormat.KEY_MAX_INPUT_SIZE, 16 * 1024);
            aac = MediaCodec.createEncoderByType(AUDIO_MIME);
            aac.configure(format, null, null, MediaCodec.CONFIGURE_FLAG_ENCODE);
            aac.start();
            MediaCodec.BufferInfo ai = new MediaCodec.BufferInfo();
            int frameBytes = 2 * channels;
            int offset = 0;
            boolean inputDone = false, outputDone = false;
            long guard = System.currentTimeMillis() + 20_000;
            while (!outputDone && System.currentTimeMillis() < guard) {
                if (!inputDone) {
                    int in = aac.dequeueInputBuffer(10_000);
                    if (in >= 0) {
                        ByteBuffer buf = aac.getInputBuffer(in);
                        long pts = (offset / frameBytes) * 1_000_000L / sampleRate;
                        if (offset >= pcm.length || buf == null) {
                            aac.queueInputBuffer(in, 0, 0, pts, MediaCodec.BUFFER_FLAG_END_OF_STREAM);
                            inputDone = true;
                        } else {
                            buf.clear();
                            int n = Math.min(buf.remaining(), pcm.length - offset);
                            n -= n % frameBytes;
                            buf.put(pcm, offset, n);
                            aac.queueInputBuffer(in, 0, n, pts, 0);
                            offset += n;
                        }
                    }
                }
                int out = aac.dequeueOutputBuffer(ai, 10_000);
                if (out == MediaCodec.INFO_OUTPUT_FORMAT_CHANGED) {
                    audioFormat = aac.getOutputFormat();
                } else if (out >= 0) {
                    ByteBuffer data = aac.getOutputBuffer(out);
                    boolean config = (ai.flags & MediaCodec.BUFFER_FLAG_CODEC_CONFIG) != 0;
                    if (!config && ai.size > 0 && data != null) samples.add(new Sample(copy(data, ai), ai.presentationTimeUs, ai.flags, true));
                    aac.releaseOutputBuffer(out, false);
                    if ((ai.flags & MediaCodec.BUFFER_FLAG_END_OF_STREAM) != 0) outputDone = true;
                }
            }
        } catch (Exception e) {
            audioFormat = null;   // niente audio: il video si salva lo stesso
            removeAudioSamples();
        } finally {
            if (aac != null) {
                try { aac.stop(); } catch (Exception ignored) { }
                aac.release();
            }
        }
    }

    private void removeAudioSamples() {
        for (int i = samples.size() - 1; i >= 0; i--) if (samples.get(i).audio) samples.remove(i);
    }

    private static byte[] copy(ByteBuffer data, MediaCodec.BufferInfo bi) {
        byte[] b = new byte[bi.size];
        data.position(bi.offset);
        data.limit(bi.offset + bi.size);
        data.get(b);
        return b;
    }

    /** Chiude il video e scrive il file MP4 (video + audio, in ordine di tempo). */
    public void finish() {
        int index = codec.dequeueInputBuffer(50_000);
        if (index >= 0) {
            long pts = frameIndex * 1_000_000L / fps;
            codec.queueInputBuffer(index, 0, 0, pts, MediaCodec.BUFFER_FLAG_END_OF_STREAM);
        }
        drainVideo(true);
        try { codec.stop(); } catch (Exception ignored) { }
        codec.release();
        if (videoFormat == null) return;

        MediaMuxer muxer = null;
        try {
            new File(path).delete();
            muxer = new MediaMuxer(path, MediaMuxer.OutputFormat.MUXER_OUTPUT_MPEG_4);
            int videoTrack = muxer.addTrack(videoFormat);
            int audioTrack = audioFormat != null ? muxer.addTrack(audioFormat) : -1;
            muxer.start();
            Collections.sort(samples, new Comparator<Sample>() {
                @Override public int compare(Sample a, Sample b) { return Long.compare(a.pts, b.pts); }
            });
            MediaCodec.BufferInfo bi = new MediaCodec.BufferInfo();
            for (Sample sm : samples) {
                if (sm.audio && audioTrack < 0) continue;
                bi.set(0, sm.data.length, sm.pts, sm.flags & ~MediaCodec.BUFFER_FLAG_END_OF_STREAM);
                muxer.writeSampleData(sm.audio ? audioTrack : videoTrack, ByteBuffer.wrap(sm.data), bi);
            }
            muxer.stop();
        } catch (Exception ignored) {
        } finally {
            if (muxer != null) muxer.release();
            samples.clear();
        }
    }

    private void fillYuv(Image image) {
        Image.Plane[] planes = image.getPlanes();
        ByteBuffer yBuf = planes[0].getBuffer(), uBuf = planes[1].getBuffer(), vBuf = planes[2].getBuffer();
        int yRow = planes[0].getRowStride(), yPix = planes[0].getPixelStride();
        int uRow = planes[1].getRowStride(), uPix = planes[1].getPixelStride();
        int vRow = planes[2].getRowStride(), vPix = planes[2].getPixelStride();
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                int c = argb[y * width + x];
                int r = (c >> 16) & 0xff, g = (c >> 8) & 0xff, b = c & 0xff;
                int Y = ((66 * r + 129 * g + 25 * b + 128) >> 8) + 16;
                yBuf.put(y * yRow + x * yPix, (byte) clamp(Y));
                if ((y & 1) == 0 && (x & 1) == 0) {
                    int U = ((-38 * r - 74 * g + 112 * b + 128) >> 8) + 128;
                    int V = ((112 * r - 94 * g - 18 * b + 128) >> 8) + 128;
                    uBuf.put((y / 2) * uRow + (x / 2) * uPix, (byte) clamp(U));
                    vBuf.put((y / 2) * vRow + (x / 2) * vPix, (byte) clamp(V));
                }
            }
        }
    }

    private static int clamp(int v) { return v < 0 ? 0 : Math.min(255, v); }

    private void drainVideo(boolean endOfStream) {
        while (true) {
            int index = codec.dequeueOutputBuffer(info, endOfStream ? 20_000 : 0);
            if (index == MediaCodec.INFO_TRY_AGAIN_LATER) {
                if (!endOfStream) return;
                continue;
            }
            if (index == MediaCodec.INFO_OUTPUT_FORMAT_CHANGED) {
                videoFormat = codec.getOutputFormat();
                continue;
            }
            if (index < 0) continue;
            ByteBuffer data = codec.getOutputBuffer(index);
            boolean config = (info.flags & MediaCodec.BUFFER_FLAG_CODEC_CONFIG) != 0;
            if (!config && info.size > 0 && data != null) samples.add(new Sample(copy(data, info), info.presentationTimeUs, info.flags, false));
            codec.releaseOutputBuffer(index, false);
            if ((info.flags & MediaCodec.BUFFER_FLAG_END_OF_STREAM) != 0) return;
        }
    }

    /**
     * Salva il video nella galleria (Movies/Gotcha) e apre il menu di condivisione di Android
     * (WhatsApp, Instagram, TikTok, ...). Restituisce false se qualcosa va storto.
     */
    /** Condivide solo un testo (es. l'invito con il link): menu di condivisione di Android. */
    public static boolean shareText(final String text, final String chooserTitle) {
        try {
            final Activity activity = UnityPlayer.currentActivity;
            activity.runOnUiThread(new Runnable() {
                @Override public void run() {
                    Intent send = new Intent(Intent.ACTION_SEND);
                    send.setType("text/plain");
                    send.putExtra(Intent.EXTRA_TEXT, text);
                    activity.startActivity(Intent.createChooser(send, chooserTitle));
                }
            });
            return true;
        } catch (Exception e) {
            return false;
        }
    }

    public static boolean share(String path, String chooserTitle, String text) {
        try {
            Activity activity = UnityPlayer.currentActivity;
            Uri uri;
            if (Build.VERSION.SDK_INT >= 29) {
                uri = saveToGallery(activity.getContentResolver(), new File(path));
            } else {
                // Android 6-9: uri del file diretto (si allenta il controllo che lo vieta dalla 7)
                StrictMode.setVmPolicy(new StrictMode.VmPolicy.Builder().build());
                uri = Uri.fromFile(new File(path));
            }
            if (uri == null) return false;
            Intent send = new Intent(Intent.ACTION_SEND);
            send.setType("video/mp4");
            send.putExtra(Intent.EXTRA_STREAM, uri);
            send.putExtra(Intent.EXTRA_TEXT, text);
            send.addFlags(Intent.FLAG_GRANT_READ_URI_PERMISSION);
            activity.startActivity(Intent.createChooser(send, chooserTitle));
            return true;
        } catch (Exception e) {
            return false;
        }
    }

    private static Uri saveToGallery(ContentResolver resolver, File file) throws Exception {
        ContentValues values = new ContentValues();
        values.put(MediaStore.Video.Media.DISPLAY_NAME, "gotcha_" + System.currentTimeMillis() + ".mp4");
        values.put(MediaStore.Video.Media.MIME_TYPE, "video/mp4");
        values.put(MediaStore.Video.Media.RELATIVE_PATH, "Movies/Gotcha");
        values.put(MediaStore.Video.Media.IS_PENDING, 1);
        Uri uri = resolver.insert(MediaStore.Video.Media.getContentUri(MediaStore.VOLUME_EXTERNAL_PRIMARY), values);
        if (uri == null) return null;
        try (InputStream in = new FileInputStream(file); OutputStream out = resolver.openOutputStream(uri)) {
            byte[] buffer = new byte[64 * 1024];
            int n;
            while ((n = in.read(buffer)) > 0) out.write(buffer, 0, n);
        }
        values.clear();
        values.put(MediaStore.Video.Media.IS_PENDING, 0);
        resolver.update(uri, values, null, null);
        return uri;
    }
}
