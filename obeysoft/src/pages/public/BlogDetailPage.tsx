import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";

import { Badge } from "../../components/ui/Badge";
import { Button } from "../../components/ui/Button";
import { Card, CardContent } from "../../components/ui/Card";
import { getPostDetail, getPosts, type PostDetail, type PostListItem } from "../../lib/api";

export function BlogDetailPage() {
  const { slug = "" } = useParams();
  const [post, setPost] = useState<PostDetail | null>(null);
  const [others, setOthers] = useState<PostListItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    (async () => {
      setLoading(true);
      setError(null);
      try {
        const [detail, list] = await Promise.all([
          getPostDetail(slug),
          getPosts({ page: 1, pageSize: 6 })
        ]);
        setPost(detail);
        setOthers(list.items.filter((item) => item.slug !== slug));
      } catch (err: any) {
        setError(err?.message ?? "İçerik yüklenemedi");
      } finally {
        setLoading(false);
      }
    })();
  }, [slug]);

  if (loading) return <div className="text-sm text-muted-foreground">İçerik yükleniyor...</div>;
  if (error) return <div className="text-sm text-destructive">{error}</div>;
  if (!post) return <div className="text-sm text-muted-foreground">Kayıt bulunamadı.</div>;

  return (
    <div className="grid gap-8 lg:grid-cols-[minmax(0,1fr)_320px]">
      <article>
        <Card className="border border-border/70 bg-card/80">
          <CardContent className="space-y-6 p-8">
            <div className="flex flex-wrap items-center gap-3 text-sm text-muted-foreground">
              <Badge variant="outline">{post.categoryName}</Badge>
              {post.publishedAt && (
                <span>{new Date(post.publishedAt).toLocaleString("tr-TR")}</span>
              )}
            </div>
            <h1 className="text-3xl font-semibold leading-tight md:text-4xl">{post.title}</h1>
            {post.summary && (
              <p className="rounded-2xl border border-border/60 bg-secondary/30 p-4 text-sm text-muted-foreground">
                {post.summary}
              </p>
            )}
            <div
              className="prose prose-invert max-w-none leading-relaxed"
              dangerouslySetInnerHTML={{ __html: post.content }}
            />
          </CardContent>
        </Card>
      </article>

      <aside className="space-y-6">
        <Card className="border border-border/70 bg-card/80">
          <CardContent className="space-y-4 p-5">
            <div className="flex items-center justify-between">
              <h3 className="text-sm font-semibold uppercase tracking-wide text-muted-foreground">
                Diğer Yazılar
              </h3>
              <Badge variant="outline">{others.length}</Badge>
            </div>
            <div className="flex flex-col gap-3 text-sm">
              {others.map((item) => (
                <Link
                  key={item.id}
                  to={`/blog/${item.slug}`}
                  className="rounded-2xl border border-transparent px-3 py-2 text-muted-foreground transition hover:border-primary/40 hover:text-primary"
                >
                  <div className="text-xs uppercase tracking-wide text-muted-foreground/80">
                    {item.categoryName}
                  </div>
                  <div className="font-semibold text-foreground">{item.title}</div>
                </Link>
              ))}
              {others.length === 0 && (
                <div className="rounded-2xl border border-dashed border-border/60 p-4 text-center text-xs text-muted-foreground">
                  Henüz başka yazı yok.
                </div>
              )}
            </div>
            <Button variant="ghost" size="sm" asChild>
              <Link to="/blog">Tüm Yazıları Gör</Link>
            </Button>
          </CardContent>
        </Card>
      </aside>
    </div>
  );
}


