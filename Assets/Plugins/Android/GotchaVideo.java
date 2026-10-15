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

/**
 * Codifica in MP4 (H.264) i fotogrammi catturati dal gioco e li condivide.
 * Uso da Unity (C#): begin(path, w, h, fps) → addJpeg(bytes) per ogni fotogramma → finish() → share(...).
 * I fotogrammi arrivano come JPEG già dritti (li comprime Unity), qui vengono convertiti in YUV 4:2:0.
 */
public class GotchaVideo {
    private static final String MIME = "video/avc";

    private final MediaCodec codec;
    private final MediaMuxer muxer;
    private final MediaCodec.BufferInfo info = new MediaCodec.BufferInfo();
    private final int width, height, fps;
    private int track = -1;
    private boolean muxerStarted;
    private long frameIndex;
    private int[] argb;

    private GotchaVideo(String path, int width, int height, int fps) throws Exception {
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
        new File(path).delete();
        muxer = new MediaMuxer(path, MediaMuxer.OutputFormat.MUXER_OUTPUT_MPEG_4);
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
        drain(false);
    }

    /** Chiude il file video. */
    public void finish() {
        int index = codec.dequeueInputBuffer(50_000);
        if (index >= 0) {
            long pts = frameIndex * 1_000_000L / fps;
            codec.queueInputBuffer(index, 0, 0, pts, MediaCodec.BUFFER_FLAG_END_OF_STREAM);
        }
        drain(true);
        try { codec.stop(); } catch (Exception ignored) { }
        codec.release();
        if (muxerStarted) {
            try { muxer.stop(); } catch (Exception ignored) { }
        }
        muxer.release();
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

    private void drain(boolean endOfStream) {
        while (true) {
            int index = codec.dequeueOutputBuffer(info, endOfStream ? 20_000 : 0);
            if (index == MediaCodec.INFO_TRY_AGAIN_LATER) {
                if (!endOfStream) return;
                continue;
            }
            if (index == MediaCodec.INFO_OUTPUT_FORMAT_CHANGED) {
                track = muxer.addTrack(codec.getOutputFormat());
                muxer.start();
                muxerStarted = true;
                continue;
            }
            if (index < 0) continue;
            ByteBuffer data = codec.getOutputBuffer(index);
            if ((info.flags & MediaCodec.BUFFER_FLAG_CODEC_CONFIG) != 0) info.size = 0;
            if (info.size > 0 && muxerStarted && data != null) {
                data.position(info.offset);
                data.limit(info.offset + info.size);
                muxer.writeSampleData(track, data, info);
            }
            codec.releaseOutputBuffer(index, false);
            if ((info.flags & MediaCodec.BUFFER_FLAG_END_OF_STREAM) != 0) return;
        }
    }

    /**
     * Salva il video nella galleria (Movies/Gotcha) e apre il menu di condivisione di Android
     * (WhatsApp, Instagram, TikTok, ...). Restituisce false se qualcosa va storto.
     */
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
