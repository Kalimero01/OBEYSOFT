import { useEffect, useState } from "react";

import { Badge } from "../../components/ui/Badge";
import { Card, CardContent } from "../../components/ui/Card";
import { getCategories, type Category } from "../../lib/api";

export function PublicCategoriesPage() {
  const [categories, setCategories] = useState<Category[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    (async () => {
      try {
        const data = await getCategories();
        setCategories(data);
      } catch (err: any) {
        setError(err?.message ?? "Kategoriler yüklenemedi");
      } finally {
        setLoading(false);
      }
    })();
  }, []);

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-semibold">Kategoriler</h1>
        <p className="text-sm text-muted-foreground">
          İçeriklerimizi kategori bazında inceleyin. Her kategori özel olarak yapılandırılmış dersler içerir.
        </p>
      </div>

      {loading && <div className="text-sm text-muted-foreground">Kategoriler getiriliyor...</div>}
      {error && <div className="text-sm text-destructive">{error}</div>}

      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
        {categories.map((category) => (
          <Card key={category.id} className="border border-border/70 bg-card/80">
            <CardContent className="space-y-3 p-5">
              <div className="flex items-center justify-between text-xs text-muted-foreground">
                <span>Slug: {category.slug}</span>
                {category.displayOrder !== undefined && (
                  <Badge variant="outline">Sıra {category.displayOrder}</Badge>
                )}
              </div>
              <h2 className="text-lg font-semibold text-foreground">{category.name}</h2>
              {category.description && (
                <p className="text-sm text-muted-foreground">{category.description}</p>
              )}
            </CardContent>
          </Card>
        ))}
        {!loading && !error && categories.length === 0 && (
          <div className="rounded-2xl border border-dashed border-border/60 p-6 text-center text-sm text-muted-foreground">
            Henüz kategori bulunmuyor.
          </div>
        )}
      </div>
    </div>
  );
}

