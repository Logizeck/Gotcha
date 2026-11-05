namespace Gotcha.UI.Logic
{
    /// <summary>Da dove si tocca la barra delle pagine alla pagina (0.81). Senza Unity: testata in PageScrubTests.</summary>
    public static class PageScrub
    {
        /// <summary>
        /// <paramref name="x"/> = distanza dal bordo sinistro della barra, <paramref name="width"/> = larghezza della
        /// barra: la barra è divisa in parti uguali, una per pagina; fuori dalla barra si resta sulla prima/ultima.
        /// </summary>
        public static int PageAt(float x, float width, int pageCount)
        {
            if (pageCount <= 1 || width <= 0f || float.IsNaN(x)) return 0;
            float t = x / width;
            if (t <= 0f) return 0;
            if (t >= 1f) return pageCount - 1;
            int page = (int)(t * pageCount);
            return page >= pageCount ? pageCount - 1 : page;
        }
    }
}
