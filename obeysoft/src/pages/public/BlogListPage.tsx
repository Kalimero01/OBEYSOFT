import { useEffect, useMemo, useState } from "react";
import { Link, useSearchParams } from "react-router-dom";

import {
  getCategories,
  getPosts,
  type Category,
  type Paginated,
  type PostListItem
} from "../../lib/api";
import { Badge } from "../../components/ui/Badge";
import { Button } from "../../components/ui/Button";
import { Card, CardContent } from "../../components/ui/Card";

export function BlogListPage() {
  const [searchParams, setSearchParams] = useSearchParams();
  const [categories, setCategories] = useState<Category[]>([]);
  const [posts, setPosts] = useState<Paginated<PostListItem>>({
    items: [],
    page: 1,
    pageSize: 10,
    totalCount: 0,
    totalPages: 1
  });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const activeCategory = searchParams.get("category") ?? undefined;
  const page = Math.max(1, Number(searchParams.get("page") ?? "1"));

  useEffect(() => {
    (async () => {
      try {
        const [cats, result] = await Promise.all([
          getCategories(),
          getPosts({ page, pageSize: 9, category: activeCategory })
        ]);
        setCategories(cats);
        setPosts(result);
      } catch (err: any) {
        setError(err?.message ?? "Veriler yüklenemedi");
      } finally {
        setLoading(false);
      }
    })();
  }, [page, activeCategory]);

  const totalPages = useMemo(() => Math.max(1, posts.totalPages ?? 1), [posts.totalPages]);

  const handleCategoryChange = (slug?: string) => {
    const params: Record<string, string> = {};
    if (slug) params.category = slug;
    params.page = "1";
    setSearchParams(params, { replace: true });
  };

  const changePage = (next: number) => {
    const params: Record<string, string> = {};
    if (activeCategory) params.category = activeCategory;
    params.page = String(next);
    setSearchParams(params, { replace: true });
  };

  return (
    <div className="grid gap-8 lg:grid-cols-[280px_minmax(0,1fr)]">
      <aside className="space-y-4">
        <Card className="border border-border/70 bg-card/80">
          <CardContent className="space-y-3 p-5">
            <div className="flex items-center justify-between">
              <h2 className="text-sm font-semibold uppercase tracking-wide text-muted-foreground">
                Kategoriler
              </h2>
              <Badge variant="outline">{categories.length}</Badge>
            </div>
            <div className="flex flex-col gap-1 text-sm">
              <button
                onClick={() => handleCategoryChange(undefined)}
                className={`rounded-xl px-3 py-2 text-left transition ${
                  activeCategory ? "text-muted-foreground hover:bg-secondary/40" : "bg-primary/15 text-primary"
                }`}
              >
                Tümü
              </button>
              {categories.map((category) => (
                <button
                  key={category.id}
                  onClick={() => handleCategoryChange(category.slug)}
                  className={`rounded-xl px-3 py-2 text-left transition ${
                    activeCategory === category.slug
                      ? "bg-primary/15 text-primary"
                      : "text-muted-foreground hover:bg-secondary/40"
                  }`}
                >
                  {category.name}
                </button>
              ))}
            </div>
          </CardContent>
        </Card>
      </aside>

      <section className="space-y-6">
        <div>
          <h1 className="text-3xl font-semibold">Blog</h1>
          <p className="text-sm text-muted-foreground">
            Teknik yazılar, ders notları ve üretim hikayeleri.
          </p>
        </div>

        {loading && <div className="text-sm text-muted-foreground">Yazılar yükleniyor...</div>}
        {error && !loading && <div className="text-sm text-destructive">{error}</div>}

        <div className="grid gap-5 md:grid-cols-2 xl:grid-cols-3">
          {posts.items.map((post) => (
            <Card key={post.id} className="h-full border border-border/70 bg-card/80 transition hover:border-primary/30">
              <CardContent className="space-y-3 p-5">
                <div className="flex items-center justify-between text-xs text-muted-foreground">
                  <span>{post.categoryName}</span>
                  <span>{new Date(post.createdAt).toLocaleDateString("tr-TR")}</span>
                </div>
                <Link
                  to={`/blog/${post.slug}`}
                  className="text-lg font-semibold leading-6 text-foreground transition hover:text-primary"
                >
                  {post.title}
                </Link>
                <p className="text-sm text-muted-foreground">
                  {post.summary ?? "Yazının tamamını okumak için detay sayfasına göz atın."}
                </p>
                <Button variant="ghost" size="sm" className="px-0" asChild>
                  <Link to={`/blog/${post.slug}`}>Devamını Oku</Link>
                </Button>
              </CardContent>
            </Card>
          ))}
        </div>

        {!loading && !error && posts.items.length === 0 && (
          <div className="rounded-2xl border border-dashed border-border/60 p-6 text-center text-sm text-muted-foreground">
            Seçtiğiniz filtreye uygun yazı bulunamadı.
          </div>
        )}

        <div className="flex items-center justify-between gap-3 rounded-2xl border border-border/60 bg-card/70 px-4 py-3 text-sm">
          <div>
            Sayfa {page} / {totalPages}
          </div>
          <div className="flex items-center gap-2">
            <Button variant="ghost" size="sm" disabled={page <= 1} onClick={() => changePage(page - 1)}>
              Önceki
            </Button>
            <Button
              variant="ghost"
              size="sm"
              disabled={page >= totalPages}
              onClick={() => changePage(page + 1)}
            >
              Sonraki
            </Button>
          </div>
        </div>
      </section>
    </div>
  );
}


