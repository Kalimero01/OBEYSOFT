import { motion } from "framer-motion";
import { ArrowRight, ArrowUpRight, BookOpen, Code2, Image, Newspaper, PlayCircle, Rocket } from "lucide-react";
import { useEffect, useMemo, useState } from "react";
import type { ReactNode } from "react";
import { Link } from "react-router-dom";

import { Badge } from "../../components/ui/Badge";
import { Button } from "../../components/ui/Button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle
} from "../../components/ui/Card";
import { cn } from "../../lib/utils";
import { getCategories, getPosts, type Category, type PostListItem } from "../../lib/api";

type BulletinSection = {
  key: string;
  title: string;
  description: string;
  icon: ReactNode;
  posts: PostListItem[];
};

export function HomePage() {
  const [categories, setCategories] = useState<Category[]>([]);
  const [posts, setPosts] = useState<PostListItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [selectedSlug, setSelectedSlug] = useState<string | null>(null);

  const selectedCategory = useMemo(
    () => categories.find((category) => category.slug === selectedSlug) ?? null,
    [categories, selectedSlug]
  );

  useEffect(() => {
    getCategories()
      .then((list) => setCategories(list ?? []))
      .catch(() => setCategories([]));
  }, []);

  useEffect(() => {
    setLoading(true);
    setError(null);

    getPosts({ page: 1, pageSize: 12, category: selectedSlug ?? undefined })
      .then((response) => {
        setPosts(response.items ?? []);
      })
      .catch((err: any) => {
        setError(err?.message ?? "Yazılar yüklenirken bir hata oluştu");
      })
      .finally(() => {
        setLoading(false);
      });
  }, [selectedSlug]);

  const featured = posts[0] ?? null;
  const timeline = posts.slice(1, 4);

  const bulletinSections: BulletinSection[] = useMemo(() => {
    const normalize = (value: string | undefined | null) => (value ?? "").toLowerCase();
    const matches = (post: PostListItem, keywords: string[]) => {
      const haystack = `${normalize(post.categoryName)} ${normalize(post.title)}`;
      return keywords.some((keyword) => haystack.includes(keyword));
    };

    return [
      {
        key: "news",
        title: "Son Haberler",
        description: "Güncel blog yazıları ve duyurular",
        icon: <Newspaper className="h-4 w-4" />,
        posts: posts.slice(0, 4)
      },
      {
        key: "lessons",
        title: "Dersler",
        description: "Adım adım eğitim içerikleri",
        icon: <BookOpen className="h-4 w-4" />,
        posts: posts.filter((post) => matches(post, ["ders", "lesson", "eğitim"])).slice(0, 4)
      },
      {
        key: "videos",
        title: "Videolar",
        description: "Kayıtlı yayınlar ve video anlatımlar",
        icon: <PlayCircle className="h-4 w-4" />,
        posts: posts.filter((post) => matches(post, ["video", "yayın", "stream"])).slice(0, 4)
      },
      {
        key: "gallery",
        title: "Resimli Yazılar",
        description: "Görsel ağırlıklı paylaşımlar",
        icon: <Image className="h-4 w-4" />,
        posts: posts.filter((post) => matches(post, ["galeri", "resim", "foto"])).slice(0, 4)
      }
    ];
  }, [posts]);

  return (
    <div className="space-y-16">
      <section className="grid items-center gap-12 md:grid-cols-[minmax(0,1.1fr)_minmax(0,0.9fr)]">
        <div className="space-y-6">
          <motion.div
            initial={{ opacity: 0, y: -16 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.45 }}
            className="space-y-6"
          >
            <span className="inline-flex items-center gap-2 rounded-full border border-primary/30 bg-primary/10 px-3 py-1 text-xs font-semibold uppercase tracking-[0.35em] text-primary">
              Yazılım · Backend · Fullstack
            </span>
            <div className="space-y-4">
              <h1 className="text-4xl font-bold leading-tight md:text-5xl">
                OBEYSOFT ile üretime hazır çözümler geliştirin
              </h1>
              <p className="text-lg text-muted-foreground">
                .NET 8, PostgreSQL ve React ekosistemini kullanarak sürdürülebilir, modern ve güvenli web
                uygulamaları tasarlıyoruz.
              </p>
            </div>
            <div className="flex flex-wrap gap-3">
              <Button size="lg" asChild>
                <Link to="/blog">
                  <Rocket size={18} className="mr-2" /> Blogu Keşfet
                </Link>
              </Button>
              <Button variant="outline" size="lg" asChild>
                <Link to="/iletisim">İletişime Geç</Link>
              </Button>
            </div>
          </motion.div>
        </div>
        <motion.div initial={{ opacity: 0, scale: 0.96 }} animate={{ opacity: 1, scale: 1 }} transition={{ duration: 0.4 }}>
          <Card className="border border-primary/30 bg-card/80 shadow-2xl">
            <CardHeader>
              <div className="flex items-center justify-between">
                <div>
                  <CardDescription>Teknoloji Yığını</CardDescription>
                  <CardTitle className="text-lg">.NET 8 · PostgreSQL · React · Tailwind</CardTitle>
                </div>
                <span className="rounded-2xl bg-primary/15 p-3 text-primary">
                  <Code2 />
                </span>
              </div>
            </CardHeader>
            <CardContent className="space-y-4">
              <pre className="overflow-hidden rounded-2xl border border-border/80 bg-background/60 p-4 text-xs text-muted-foreground shadow-inner">
{`public sealed class ObeysoftApi
{
    public void ConfigureServices()
    {
        AddAuthentication();
        AddPostgres();
        AddBlogFeatures();
    }
}`}
              </pre>
              <div className="flex flex-wrap gap-2">
                {["Clean Architecture", "DDD", "Render Deploy", "Vercel Frontend"].map((tag) => (
                  <Badge key={tag} variant="outline">
                    {tag}
                  </Badge>
                ))}
              </div>
            </CardContent>
          </Card>
        </motion.div>
      </section>

      <section className="grid gap-10 lg:grid-cols-[280px_minmax(0,1fr)] xl:grid-cols-[300px_minmax(0,1fr)]">
        <aside className="space-y-6">
          <Card className="border border-border/70 bg-secondary/30">
            <CardHeader className="space-y-2">
              <Badge variant="outline" className="w-fit">
                Kategoriler
              </Badge>
              <CardTitle className="text-xl">İçerikleri Keşfedin</CardTitle>
              <CardDescription>İlgilendiğiniz konu başlığını seçerek bülteni filtreleyin.</CardDescription>
            </CardHeader>
            <CardContent className="space-y-1 pt-0">
              <button
                type="button"
                onClick={() => setSelectedSlug(null)}
                className={cn(
                  "flex w-full items-center justify-between rounded-xl px-3 py-2 text-sm font-medium transition",
                  selectedSlug === null
                    ? "bg-primary/20 text-foreground shadow-[inset_0_0_0_1px_rgba(59,130,246,0.35)]"
                    : "text-muted-foreground hover:bg-secondary/40 hover:text-foreground"
                )}
              >
                <span>Tümü</span>
                <ArrowRight className="h-4 w-4" />
              </button>
              {categories.map((category) => {
                const active = category.slug === selectedSlug;
                return (
                  <button
                    key={category.id}
                    type="button"
                    onClick={() => setSelectedSlug((prev) => (prev === category.slug ? null : category.slug))}
                    className={cn(
                      "flex w-full items-center justify-between rounded-xl px-3 py-2 text-sm transition",
                      active
                        ? "bg-primary/20 text-foreground shadow-[inset_0_0_0_1px_rgba(59,130,246,0.35)]"
                        : "text-muted-foreground hover:bg-secondary/40 hover:text-foreground"
                    )}
                  >
                    <span>{category.name}</span>
                    <ArrowRight className="h-4 w-4" />
                  </button>
                );
              })}
            </CardContent>
          </Card>
        </aside>

        <div className="space-y-8">
          <div className="flex flex-wrap items-center justify-between gap-3">
            <div>
              <h2 className="text-2xl font-semibold">Güncel Bülten</h2>
              <p className="text-sm text-muted-foreground">
                Son gönderilen haber, ders, video ve görseller tek noktada.
              </p>
            </div>
            <Button variant="ghost" size="sm" asChild>
              <Link to="/blog">
                Tüm Yazılar <ArrowUpRight size={16} className="ml-1" />
              </Link>
            </Button>
          </div>

          {loading && (
            <div className="rounded-3xl border border-dashed border-border/60 p-10 text-center text-sm text-muted-foreground">
              İçerikler yükleniyor...
            </div>
          )}

          {error && !loading && (
            <div className="rounded-3xl border border-destructive/40 bg-destructive/10 p-6 text-sm text-destructive">
              {error}
            </div>
          )}

          {!loading && !error && selectedCategory && (
            <div className="space-y-6">
              <Card className="border border-primary/25 bg-primary/10">
                <CardHeader>
                  <Badge variant="outline" className="w-fit">
                    {selectedCategory.name}
                  </Badge>
                  <CardTitle className="text-2xl">{selectedCategory.name} içerikleri</CardTitle>
                  <CardDescription>
                    {selectedCategory.description ?? "Seçtiğiniz kategoriye ait güncel yazılar"}
                  </CardDescription>
                </CardHeader>
              </Card>

              {posts.length === 0 && (
                <div className="rounded-3xl border border-dashed border-border/60 p-10 text-center text-sm text-muted-foreground">
                  Bu kategoriye ait içerik henüz eklenmemiş.
                </div>
              )}

              <div className="grid gap-5 md:grid-cols-2">
                {posts.map((post) => (
                  <Card key={post.id} className="border border-border/70 bg-card/80 transition hover:-translate-y-1 hover:border-primary/40 hover:shadow-lg">
                    <CardHeader className="space-y-2">
                      <CardDescription>
                        {new Date(post.createdAt).toLocaleDateString("tr-TR")}
                      </CardDescription>
                      <CardTitle className="text-xl">
                        <Link to={`/blog/${post.slug}`} className="transition hover:text-primary">
                          {post.title}
                        </Link>
                      </CardTitle>
                      <CardDescription>{post.summary ?? "Detayları okumak için devam edin."}</CardDescription>
                    </CardHeader>
                    <CardContent className="pt-0">
                      <Button variant="ghost" size="sm" asChild>
                        <Link to={`/blog/${post.slug}`}>
                          Devamını oku <ArrowUpRight size={16} className="ml-1" />
                        </Link>
                      </Button>
                    </CardContent>
                  </Card>
                ))}
              </div>
            </div>
          )}

          {!loading && !error && !selectedCategory && (
            <div className="space-y-10">
              {featured && (
                <Card className="overflow-hidden border border-primary/25 bg-gradient-to-br from-primary/10 via-background to-background">
                  <CardHeader className="relative space-y-3">
                    <Badge variant="outline" className="w-fit">
                      Öne Çıkan Gönderi
                    </Badge>
                    <CardTitle className="text-3xl">
                      <Link to={`/blog/${featured.slug}`} className="transition hover:text-primary">
                        {featured.title}
                      </Link>
                    </CardTitle>
                    <CardDescription>
                      {new Date(featured.createdAt).toLocaleDateString("tr-TR")} · {featured.categoryName}
                    </CardDescription>
                    <CardDescription>
                      {featured.summary ?? "Yazının tamamını incelemek için hemen göz atın."}
                    </CardDescription>
                    <div className="pt-2">
                      <Button asChild>
                        <Link to={`/blog/${featured.slug}`}>
                          Haberi Oku <ArrowUpRight size={16} className="ml-1" />
                        </Link>
                      </Button>
                    </div>
                  </CardHeader>
                </Card>
              )}

              {timeline.length > 0 && (
                <div className="grid gap-5 md:grid-cols-2 xl:grid-cols-3">
                  {timeline.map((post) => (
                    <Card key={post.id} className="border border-border/70 bg-card/80 transition hover:-translate-y-1 hover:border-primary/40 hover:shadow-lg">
                      <CardHeader className="space-y-2">
                        <CardDescription>
                          {post.categoryName} · {new Date(post.createdAt).toLocaleDateString("tr-TR")}
                        </CardDescription>
                        <CardTitle className="text-lg">
                          <Link to={`/blog/${post.slug}`} className="transition hover:text-primary">
                            {post.title}
                          </Link>
                        </CardTitle>
                        <CardDescription>{post.summary ?? "Detayları okumak için devam edin."}</CardDescription>
                      </CardHeader>
                    </Card>
                  ))}
                </div>
              )}

              <div className="grid gap-5 lg:grid-cols-2">
                {bulletinSections.map((section) => (
                  <Card key={section.key} className="border border-border/70 bg-card/70">
                    <CardHeader className="flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
                      <div className="flex items-center gap-2">
                        <div className="flex h-9 w-9 items-center justify-center rounded-xl bg-secondary/50 text-primary">
                          {section.icon}
                        </div>
                        <div>
                          <CardTitle className="text-lg">{section.title}</CardTitle>
                          <CardDescription>{section.description}</CardDescription>
                        </div>
                      </div>
                      <Badge variant="outline" className="hidden sm:inline-flex">
                        {section.posts.length} içerik
                      </Badge>
                    </CardHeader>
                    <CardContent className="space-y-3 pt-0">
                      {section.posts.length === 0 && (
                        <div className="rounded-2xl border border-dashed border-border/60 p-4 text-sm text-muted-foreground">
                          Şu an için içerik bulunmuyor.
                        </div>
                      )}
                      {section.posts.map((post) => (
                        <div
                          key={post.id}
                          className="flex items-start justify-between gap-4 rounded-2xl border border-transparent px-3 py-2 transition hover:border-primary/30 hover:bg-secondary/40"
                        >
                          <div>
                            <Link to={`/blog/${post.slug}`} className="font-medium transition hover:text-primary">
                              {post.title}
                            </Link>
                            <div className="text-xs text-muted-foreground">
                              {new Date(post.createdAt).toLocaleDateString("tr-TR")}
                            </div>
                          </div>
                          <ArrowUpRight className="mt-1 h-4 w-4 text-muted-foreground" />
                        </div>
                      ))}
                    </CardContent>
                  </Card>
                ))}
              </div>
            </div>
          )}
        </div>
      </section>
    </div>
  );
}


