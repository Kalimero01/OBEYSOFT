import { Plus, Save, Trash2 } from "lucide-react";
import { useEffect, useMemo, useState } from "react";

import {
  adminCategoriesList,
  adminCategoryDelete,
  adminCategoryUpsert,
  type Category
} from "../../lib/api";
import { Badge } from "../../components/ui/Badge";
import { Button } from "../../components/ui/Button";
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "../../components/ui/Card";
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

type CategoryFormState = {
  name: string;
  slug: string;
  description: string;
  parentId: string;
  displayOrder: number;
  isActive: boolean;
};

const emptyCategory: CategoryFormState = {
  name: "",
  slug: "",
  description: "",
  parentId: "",
  displayOrder: 1,
  isActive: true
};

export function CategoriesPage() {
  const [categories, setCategories] = useState<Category[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [formOpen, setFormOpen] = useState(false);
  const [form, setForm] = useState<CategoryFormState>(emptyCategory);
  const [editId, setEditId] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);

  const parentOptions = useMemo(
    () => categories.filter((category) => !category.parentId),
    [categories]
  );

  const load = async () => {
    setLoading(true);
    setError(null);
    try {
      const rows = await adminCategoriesList();
      setCategories(rows);
    } catch (err: any) {
      setError(err?.message ?? "Kategoriler alınamadı");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load();
  }, []);

  const openForm = (category?: Category) => {
    if (category) {
      setEditId(category.id);
      setForm({
        name: category.name,
        slug: category.slug,
        description: category.description ?? "",
        parentId: category.parentId ?? "",
        displayOrder: category.displayOrder ?? 1,
        isActive: category.isActive ?? true
      });
    } else {
      setEditId(null);
      setForm(emptyCategory);
    }
    setFormOpen(true);
  };

  const handleSave = async () => {
    if (!form.name) return;
    setSaving(true);
    try {
      await adminCategoryUpsert(editId, {
        name: form.name.trim(),
        slug:
          form.slug.trim() ||
          form.name
            .toLowerCase()
            .replace(/[^a-z0-9]+/g, "-")
            .replace(/^-+|-+$/g, ""),
        description: form.description.trim() || null,
        parentId: form.parentId ? form.parentId : null,
        displayOrder: form.displayOrder,
        isActive: form.isActive
      });
      setFormOpen(false);
      setForm(emptyCategory);
      await load();
    } catch (err: any) {
      setError(err?.message ?? "Kategori kaydedilemedi");
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async (id: string) => {
    await adminCategoryDelete(id);
    await load();
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-semibold">Kategoriler</h1>
          <p className="text-sm text-muted-foreground">Menü ve içerik yapınızı yönetin.</p>
        </div>
        <Button onClick={() => openForm()}>
          <Plus size={16} className="mr-2" /> Yeni Kategori
        </Button>
      </div>

      {error && (
        <div className="rounded-2xl border border-destructive/50 bg-destructive/10 p-3 text-sm text-destructive">
          {error}
        </div>
      )}

      <div className="overflow-hidden rounded-3xl border border-border/70 bg-card/80">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Ad</TableHead>
              <TableHead>Slug</TableHead>
              <TableHead>Üst Kategori</TableHead>
              <TableHead>Sıra</TableHead>
              <TableHead>Durum</TableHead>
              <TableHead className="text-right">İşlemler</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {loading && (
              <TableRow>
                <TableCell colSpan={6} className="py-6 text-center text-sm text-muted-foreground">
                  Veriler yükleniyor...
                </TableCell>
              </TableRow>
            )}
            {!loading && categories.length === 0 && (
              <TableRow>
                <TableCell colSpan={6} className="py-6 text-center text-sm text-muted-foreground">
                  Kayıt bulunamadı.
                </TableCell>
              </TableRow>
            )}
            {categories.map((category) => (
              <TableRow key={category.id}>
                <TableCell className="font-medium text-foreground">{category.name}</TableCell>
                <TableCell>{category.slug}</TableCell>
                <TableCell>{categories.find((c) => c.id === category.parentId)?.name ?? "-"}</TableCell>
                <TableCell>{category.displayOrder ?? 0}</TableCell>
                <TableCell>
                  <Badge variant={category.isActive ? "success" : "warning"}>
                    {category.isActive ? "Aktif" : "Pasif"}
                  </Badge>
                </TableCell>
                <TableCell className="text-right">
                  <div className="inline-flex items-center gap-2">
                    <Button variant="ghost" size="sm" onClick={() => openForm(category)}>
                      Düzenle
                    </Button>
                    <Button
                      variant="ghost"
                      size="icon"
                      className="text-destructive hover:text-destructive"
                      onClick={() => handleDelete(category.id)}
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

      {formOpen && (
        <Card className="border border-primary/35 bg-card/90 shadow-lg">
          <CardHeader className="space-y-2">
            <Badge variant="outline" className="w-fit">
              {editId ? "Düzenleme" : "Yeni Kayıt"}
            </Badge>
            <CardTitle>{editId ? "Kategori Düzenle" : "Yeni Kategori"}</CardTitle>
            <CardDescription>Menü ve içerik filtrelerini burada yapılandırın.</CardDescription>
          </CardHeader>
          <CardContent className="grid gap-4">
            <div className="grid gap-2">
              <Label htmlFor="name">Ad</Label>
              <Input
                id="name"
                value={form.name}
                onChange={(event) => setForm((prev) => ({ ...prev, name: event.currentTarget.value }))}
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
              <Label htmlFor="parent">Üst Kategori</Label>
              <select
                id="parent"
                className="h-10 w-full rounded-xl border border-input bg-secondary/30 px-3 text-sm text-foreground focus:outline-none focus:ring-2 focus:ring-ring"
                value={form.parentId}
                onChange={(event) => setForm((prev) => ({ ...prev, parentId: event.currentTarget.value }))}
              >
                <option value="">Üst kategori yok</option>
                {parentOptions.map((category) => (
                  <option key={category.id} value={category.id}>
                    {category.name}
                  </option>
                ))}
              </select>
            </div>
            <div className="grid gap-2">
              <Label htmlFor="order">Görüntüleme Sırası</Label>
              <Input
                id="order"
                type="number"
                min={0}
                value={String(form.displayOrder)}
                onChange={(event) => setForm((prev) => ({ ...prev, displayOrder: Number(event.currentTarget.value) }))}
              />
            </div>
            <div className="grid gap-2">
              <Label htmlFor="description">Açıklama</Label>
              <Input
                id="description"
                value={form.description}
                onChange={(event) => setForm((prev) => ({ ...prev, description: event.currentTarget.value }))}
              />
            </div>
            <div className="flex items-center justify-between rounded-2xl border border-border/60 bg-secondary/30 px-4 py-3">
              <div>
                <div className="text-sm font-medium text-foreground">Aktif</div>
                <div className="text-xs text-muted-foreground">Aktif olmayan kategoriler menüde gösterilmez.</div>
              </div>
              <Switch
                checked={form.isActive}
                onCheckedChange={(checked) => setForm((prev) => ({ ...prev, isActive: !!checked }))}
              />
            </div>
          </CardContent>
          <CardFooter className="flex items-center justify-end gap-2">
            <Button
              variant="ghost"
              onClick={() => {
                setFormOpen(false);
                setForm(emptyCategory);
              }}
            >
              İptal
            </Button>
            <Button onClick={handleSave} disabled={saving}>
              <Save size={16} className="mr-2" /> {saving ? "Kaydediliyor..." : "Kaydet"}
            </Button>
          </CardFooter>
        </Card>
      )}
    </div>
  );
}

