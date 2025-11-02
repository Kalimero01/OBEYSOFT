import { Card, CardContent } from "../../components/ui/Card";

const lessons = [
  {
    title: ".NET 8 ile REST API",
    description: "Katmanlı mimari, DDD ve Entity Framework Core ile üretim hazır REST API geliştirme.",
    level: "Orta"
  },
  {
    title: "PostgreSQL Performans İpuçları",
    description: "Migration, indeksleme ve sorgu optimizasyonu ile PostgreSQL'i ölçeklendirin.",
    level: "İleri"
  },
  {
    title: "React + Vite ile Modern Frontend",
    description: "Vite, Tailwind ve shadcn/ui kullanarak animasyonlu, responsive arayüzler geliştirme.",
    level: "Orta"
  },
  {
    title: "Render & Vercel Dağıtım Rehberi",
    description: "Render üzerinde .NET API, Vercel üzerinde Vite tabanlı frontend dağıtımı.",
    level: "Başlangıç"
  }
];

export function LessonsPage() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-semibold">Dersler</h1>
        <p className="text-sm text-muted-foreground">
          Teoriyi pratiğe dönüştüren özel içerikler. Yakında video ve canlı yayınlarla desteklenecek.
        </p>
      </div>

      <div className="grid gap-4 md:grid-cols-2">
        {lessons.map((lesson) => (
          <Card key={lesson.title} className="border border-border/70 bg-card/80">
            <CardContent className="space-y-3 p-5">
              <div className="flex items-center justify-between text-xs text-muted-foreground">
                <span>Seviye</span>
                <span className="rounded-full bg-secondary/40 px-2 py-0.5 text-[11px] uppercase tracking-wide">
                  {lesson.level}
                </span>
              </div>
              <h2 className="text-lg font-semibold text-foreground">{lesson.title}</h2>
              <p className="text-sm text-muted-foreground">{lesson.description}</p>
            </CardContent>
          </Card>
        ))}
      </div>
    </div>
  );
}

