import { useEffect, useMemo, useState } from "react";
import { Edit2, Eye, EyeOff, Plus, Save, Search, Trash2 } from "lucide-react";

import {
  adminCategoriesList,
  adminPostCreate,
  adminPostDelete,
  adminPostPublish,
  adminPostUpdate,
  adminPostsList,
  type Category,
  type Paginated,
  type PostListItem
} from "../../lib/api";
import { Badge } from "../../components/ui/Badge";
import { Button } from "../../components/ui/Button";
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "../../components/ui/Card";
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle
} from "../../components/ui/Dialog";
import { Input } from "../../components/ui/Input";
import { Label } from "../../components/ui/Label";
import { Switch } from "../../components/ui/Switch";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow
} from "../../components/ui/Table";
import { Textarea } from "../../components/ui/Textarea";

type PostFormState = {
  title: string;
  slug: string;
  summary: string;
  content: string;
  categoryId: string;
  isActive: boolean;
};

const emptyForm: PostFormState = {
  title: "",
  slug: "",
  summary: "",
  content: "",
  categoryId: "",
  isActive: true
};

export function PostsPage() {
  const [query, setQuery] = useState("");
  const [page, setPage] = useState(1);
  const [data, setData] = useState<Paginated<PostListItem>>({
    items: [],
    page: 1,
    pageSize: 10,
    totalCount: 0,
    totalPages: 1
  });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [confirmId, setConfirmId] = useState<string | null>(null);
  const [formOpen, setFormOpen] = useState(false);
  const [form, setForm] = useState<PostFormState>(emptyForm);
  const [editId, setEditId] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);
  const [categories, setCategories] = useState<Category[]>([]);

  const load = async (selectedPage = page, searchValue = query) => {
    setLoading(true);
    setError(null);
    try {
      const response = await adminPostsList({
        page: selectedPage,
        pageSize: 10,
        search: searchValue ? searchValue : undefined
      });
      setData(response);
    } catch (err: any) {
      setError(err?.message ?? "Veriler alınamadı");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load(page, query);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [page]);

  useEffect(() => {
    adminCategoriesList()
      .then(setCategories)
      .catch(() => setCategories([]));
  }, []);

  const totalPages = useMemo(() => Math.max(1, data.totalPages ?? 1), [data.totalPages]);

  const openCreate = () => {
    setEditId(null);
    setForm({
      title: "",
      slug: "",
      summary: "",
      content: "",
      categoryId: "",
      isActive: true
    });
    setFormOpen(true);
  };

  const openEdit = (post: PostListItem) => {
    setEditId(post.id);
    setForm({
      title: post.title,
      slug: post.slug,
      summary: post.summary ?? "",
      content: post.content ?? "",
      categoryId: post.categoryId ?? "",
      isActive: post.isActive ?? true
    });
    setFormOpen(true);
  };

  const handleSubmit = async () => {
    if (!form.title || !form.categoryId) return;
    setSaving(true);
    setError(null);
    try {
      const payload = {
        title: form.title.trim(),
        slug:
          form.slug.trim() ||
          form.title
            .toLowerCase()
            .replace(/[^a-z0-9]+/g, "-")
            .replace(/^-+|-+$/g, ""),
        summary: form.summary.trim() || null,
        content: form.content.trim(),
        categoryId: form.categoryId,
        isActive: form.isActive
      };

      if (editId) await adminPostUpdate(editId, payload);
      else await adminPostCreate(payload);

      setFormOpen(false);
      setForm(emptyForm);
      await load(page, query);
    } catch (err: any) {
      setError(err?.message ?? "Kayıt yapılamadı");
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async () => {
    if (!confirmId) return;
    try {
      await adminPostDelete(confirmId);
      setConfirmId(null);
      await load();
    } catch (err: any) {
      setError(err?.message ?? "Silme işlemi başarısız oldu");
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-center gap-2">
        <Button onClick={openCreate}>
          <Plus size={16} className="mr-2" /> Yeni Yazı
        </Button>
        <div className="ml-auto flex flex-wrap items-center gap-2">
          <div className="relative">
            <Search className="absolute left-3 top-2.5 h-4 w-4 text-muted-foreground" />
            <Input
              className="w-64 pl-9"
              placeholder="Başlıkta ara..."
              value={query}
              onChange={(event) => setQuery(event.currentTarget.value)}
              onKeyDown={(event) => {
                if (event.key === "Enter") {
                  event.preventDefault();
                  setPage(1);
                  load(1, event.currentTarget.value);
                }
              }}
            />
          </div>
          <Button variant="ghost" onClick={() => { setPage(1); load(1, query); }}>
            Ara
          </Button>
        </div>
      </div>

      {error && <div className="rounded-2xl border border-destructive/50 bg-destructive/10 p-3 text-sm text-destructive">{error}</div>}

      <div className="overflow-hidden rounded-3xl border border-border/70 bg-card/80">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Başlık</TableHead>
              <TableHead>Kategori</TableHead>
              <TableHead>Durum</TableHead>
              <TableHead>Oluşturma</TableHead>
              <TableHead className="text-right">İşlemler</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {loading && (
              <TableRow>
                <TableCell colSpan={5} className="py-6 text-center text-sm text-muted-foreground">
                  Veriler yükleniyor...
                </TableCell>
              </TableRow>
            )}
            {!loading && data.items.length === 0 && (
              <TableRow>
                <TableCell colSpan={5} className="py-6 text-center text-sm text-muted-foreground">
                  Kayıt bulunamadı.
                </TableCell>
              </TableRow>
            )}
            {data.items.map((post) => (
              <TableRow key={post.id}>
                <TableCell className="font-medium text-foreground">{post.title}</TableCell>
                <TableCell>{post.categoryName}</TableCell>
                <TableCell>
                  <Badge variant={post.isPublished ? "success" : "warning"}>
                    {post.isPublished ? "Yayında" : "Taslak"}
                  </Badge>
                </TableCell>
                <TableCell>{new Date(post.createdAt).toLocaleString("tr-TR")}</TableCell>
                <TableCell className="text-right">
                  <div className="inline-flex items-center gap-2">
                    <Button variant="ghost" size="icon" onClick={() => openEdit(post)}>
                      <Edit2 size={16} />
                    </Button>
                    <Button
                      variant="ghost"
                      size="icon"
                      onClick={async () => {
                        await adminPostPublish(post.id, !post.isPublished);
                        await load();
                      }}
                    >
                      {post.isPublished ? <EyeOff size={16} /> : <Eye size={16} />}
                    </Button>
                    <Button
                      variant="ghost"
                      size="icon"
                      className="text-destructive hover:text-destructive"
                      onClick={() => setConfirmId(post.id)}
                    >
                      <Trash2 size={16} />
                    </Button>
                  </div>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>

      <div className="flex items-center justify-between rounded-2xl border border-border/70 bg-card/80 px-4 py-3 text-sm">
        <span>
          Sayfa {data.page} / {totalPages}
        </span>
        <div className="flex items-center gap-2">
          <Button variant="ghost" size="sm" disabled={page <= 1} onClick={() => setPage((p) => Math.max(1, p - 1))}>
            Önceki
          </Button>
          <Button
            variant="ghost"
            size="sm"
            disabled={page >= totalPages}
            onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
          >
            Sonraki
          </Button>
        </div>
      </div>

      <Dialog open={!!confirmId} onOpenChange={(open) => !open && setConfirmId(null)}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Yazıyı silmek üzeresiniz</DialogTitle>
          </DialogHeader>
          <p className="text-sm text-muted-foreground">
            Bu işlem geri alınamaz. Yazı kalıcı olarak silinecek.
          </p>
          <DialogFooter>
            <Button variant="ghost" onClick={() => setConfirmId(null)}>
              Vazgeç
            </Button>
            <Button variant="destructive" onClick={handleDelete}>
              Sil
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {formOpen && (
        <Card className="border border-primary/35 bg-card/90 shadow-lg">
          <CardHeader className="space-y-2">
            <Badge variant="outline" className="w-fit">
              {editId ? "Düzenleme" : "Yeni Kayıt"}
            </Badge>
            <CardTitle>{editId ? "Yazıyı Düzenle" : "Yeni Yazı"}</CardTitle>
            <CardDescription>
              Başlık, içerik ve kategori bilgilerini girerek yayın akışını güncelleyin.
            </CardDescription>
          </CardHeader>
          <CardContent className="grid gap-4">
            <div className="grid gap-2">
              <Label htmlFor="title">Başlık</Label>
              <Input
                id="title"
                value={form.title}
                onChange={(event) => setForm((prev) => ({ ...prev, title: event.currentTarget.value }))}
              />
            </div>
            <div className="grid gap-2">
              <Label htmlFor="slug">Slug</Label>
              <Input
                id="slug"
                value={form.slug}
                placeholder="boş bırakılırsa otomatik"
                onChange={(event) => setForm((prev) => ({ ...prev, slug: event.currentTarget.value }))}
              />
            </div>
            <div className="grid gap-2">
              <Label htmlFor="category">Kategori</Label>
              <select
                id="category"
                className="h-10 w-full rounded-xl border border-input bg-secondary/30 px-3 text-sm text-foreground focus:outline-none focus:ring-2 focus:ring-ring"
                value={form.categoryId}
                onChange={(event) => setForm((prev) => ({ ...prev, categoryId: event.currentTarget.value }))}
              >
                <option value="">Kategori seçin</option>
                {categories.map((category) => (
                  <option key={category.id} value={category.id}>
                    {category.name}
                  </option>
                ))}
              </select>
            </div>
            <div className="grid gap-2">
              <Label htmlFor="summary">Özet</Label>
              <Input
                id="summary"
                value={form.summary}
                onChange={(event) => setForm((prev) => ({ ...prev, summary: event.currentTarget.value }))}
              />
            </div>
            <div className="grid gap-2">
              <Label htmlFor="content">İçerik</Label>
              <Textarea
                id="content"
                className="min-h-[220px]"
                value={form.content}
                onChange={(event) => setForm((prev) => ({ ...prev, content: event.currentTarget.value }))}
              />
            </div>
            <div className="flex items-center justify-between rounded-2xl border border-border/60 bg-secondary/30 px-4 py-3">
              <div>
                <div className="text-sm font-medium text-foreground">Aktif</div>
                <div className="text-xs text-muted-foreground">Aktif olmayan yazılar listelerde görünmez.</div>
              </div>
              <Switch checked={form.isActive} onCheckedChange={(checked) => setForm((prev) => ({ ...prev, isActive: !!checked }))} />
            </div>
          </CardContent>
          <CardFooter className="flex items-center justify-end gap-2">
            <Button
              variant="ghost"
              onClick={() => {
                setFormOpen(false);
                setForm(emptyForm);
              }}
            >
              İptal
            </Button>
            <Button onClick={handleSubmit} disabled={saving}>
              <Save size={16} className="mr-2" /> {saving ? "Kaydediliyor..." : "Kaydet"}
            </Button>
          </CardFooter>
        </Card>
      )}
    </div>
  );
}


