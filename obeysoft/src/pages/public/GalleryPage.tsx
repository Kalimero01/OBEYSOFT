const galleryItems = Array.from({ length: 9 }).map((_, idx) => ({
  id: idx,
  title: `Proje ${idx + 1}`,
  description: "Render & Vercel altyapısı ile yayınlanan referans çalışma.",
  gradient: ["from-sky-500/40", "from-violet-500/40", "from-emerald-500/40"][idx % 3]
}));

export function GalleryPage() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-semibold">Galeri</h1>
        <p className="text-sm text-muted-foreground">
          Üzerinde çalıştığımız projelerden bazı ekran görüntüleri ve konsept kartlar.
        </p>
      </div>

      <div className="grid gap-5 sm:grid-cols-2 xl:grid-cols-3">
        {galleryItems.map((item) => (
          <div
            key={item.id}
            className={`group relative overflow-hidden rounded-3xl border border-border/60 bg-background/60 p-6 shadow-lg transition hover:border-primary/40`}
          >
            <div
              className={`absolute inset-0 bg-gradient-to-br ${item.gradient} via-transparent to-transparent opacity-0 transition group-hover:opacity-40`}
            />
            <div className="relative space-y-3">
              <div className="text-sm text-muted-foreground">OBEYSOFT Showcase</div>
              <h2 className="text-xl font-semibold">{item.title}</h2>
              <p className="text-sm text-muted-foreground">{item.description}</p>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}

