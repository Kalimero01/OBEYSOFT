import { Plus, Save, Trash2 } from "lucide-react";
import { useEffect, useMemo, useState } from "react";

import {
  adminNavigationDelete,
  adminNavigationList,
  adminNavigationUpsert,
  type NavigationItemRow
} from "../../lib/api";
import { Badge } from "../../components/ui/Badge";
import { Button } from "../../components/ui/Button";
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

type NavigationFormState = {
  label: string;
  href: string;
  parentId: string;
  displayOrder: number;
  isActive: boolean;
};

const emptyNav: NavigationFormState = {
  label: "",
  href: "",
  parentId: "",
  displayOrder: 1,
  isActive: true
};

export function NavigationPage() {
  const [items, setItems] = useState<NavigationItemRow[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [formOpen, setFormOpen] = useState(false);
  const [form, setForm] = useState<NavigationFormState>(emptyNav);
  const [editId, setEditId] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);

  const parentOptions = useMemo(
    () => items.filter((item) => !item.parentId),
    [items]
  );

  const load = async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await adminNavigationList();
      setItems(data);
    } catch (err: any) {
      setError(err?.message ?? "Menü öğeleri alınamadı");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load();
  }, []);

  const openForm = (item?: NavigationItemRow) => {
    if (item) {
      setEditId(item.id);
      setForm({
        label: item.label,
        href: item.href,
        parentId: item.parentId ?? "",
        displayOrder: item.displayOrder,
        isActive: item.isActive
      });
    } else {
      setEditId(null);
      setForm(emptyNav);
    }
    setFormOpen(true);
  };

  const handleSave = async () => {
    if (!form.label || !form.href) return;
    setSaving(true);
    try {
      await adminNavigationUpsert(editId, {
        label: form.label.trim(),
        href: form.href.trim(),
        parentId: form.parentId ? form.parentId : null,
        displayOrder: form.displayOrder,
        isActive: form.isActive
      });
      setFormOpen(false);
      setForm(emptyNav);
      await load();
    } catch (err: any) {
      setError(err?.message ?? "Menü öğesi kaydedilemedi");
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async (id: string) => {
    await adminNavigationDelete(id);
    await load();
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-semibold">Üst Menü</h1>
          <p className="text-sm text-muted-foreground">Site navigasyon öğelerini yönetin.</p>
        </div>
        <Button onClick={() => openForm()}>
          <Plus size={16} className="mr-2" /> Yeni Menü Öğesi
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
              <TableHead>Etiket</TableHead>
              <TableHead>Bağlantı</TableHead>
              <TableHead>Üst Öğesi</TableHead>
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
            {!loading && items.length === 0 && (
              <TableRow>
                <TableCell colSpan={6} className="py-6 text-center text-sm text-muted-foreground">
                  Menü öğesi bulunamadı.
                </TableCell>
              </TableRow>
            )}
            {items.map((item) => (
              <TableRow key={item.id}>
                <TableCell className="font-medium text-foreground">{item.label}</TableCell>
                <TableCell>{item.href}</TableCell>
                <TableCell>{items.find((x) => x.id === item.parentId)?.label ?? "-"}</TableCell>
                <TableCell>{item.displayOrder}</TableCell>
                <TableCell>
                  <Badge variant={item.isActive ? "success" : "warning"}>
                    {item.isActive ? "Aktif" : "Pasif"}
                  </Badge>
                </TableCell>
                <TableCell className="text-right">
                  <div className="inline-flex items-center gap-2">
                    <Button variant="ghost" size="sm" onClick={() => openForm(item)}>
                      Düzenle
                    </Button>
                    <Button
                      variant="ghost"
                      size="icon"
                      className="text-destructive hover:text-destructive"
                      onClick={() => handleDelete(item.id)}
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

      <Dialog open={formOpen} onOpenChange={(open) => { if (!open) setFormOpen(false); }}>
        <DialogContent className="max-w-lg">
          <DialogHeader>
            <DialogTitle>{editId ? "Menü Öğesini Düzenle" : "Yeni Menü Öğesi"}</DialogTitle>
          </DialogHeader>
          <div className="grid gap-4 py-2">
            <div className="grid gap-2">
              <Label htmlFor="label">Etiket</Label>
              <Input
                id="label"
                value={form.label}
                onChange={(event) => setForm((prev) => ({ ...prev, label: event.currentTarget.value }))}
              />
            </div>
            <div className="grid gap-2">
              <Label htmlFor="href">Bağlantı</Label>
              <Input
                id="href"
                placeholder="/blog"
                value={form.href}
                onChange={(event) => setForm((prev) => ({ ...prev, href: event.currentTarget.value }))}
              />
            </div>
            <div className="grid gap-2">
              <Label htmlFor="parent">Üst Öğesi</Label>
              <select
                id="parent"
                className="h-10 w-full rounded-xl border border-input bg-secondary/30 px-3 text-sm text-foreground focus:outline-none focus:ring-2 focus:ring-ring"
                value={form.parentId}
                onChange={(event) => setForm((prev) => ({ ...prev, parentId: event.currentTarget.value }))}
              >
                <option value="">Üst öğe yok</option>
                {parentOptions.map((item) => (
                  <option key={item.id} value={item.id}>
                    {item.label}
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
            <div className="flex items-center justify-between rounded-2xl border border-border/60 bg-secondary/30 px-4 py-3">
              <div>
                <div className="text-sm font-medium text-foreground">Aktif</div>
                <div className="text-xs text-muted-foreground">Aktif olmayan öğeler menüde görünmez.</div>
              </div>
              <Switch checked={form.isActive} onCheckedChange={(checked) => setForm((prev) => ({ ...prev, isActive: !!checked }))} />
            </div>
          </div>
          <DialogFooter>
            <Button variant="ghost" onClick={() => setFormOpen(false)}>
              İptal
            </Button>
            <Button onClick={handleSave} disabled={saving}>
              <Save size={16} className="mr-2" /> {saving ? "Kaydediliyor..." : "Kaydet"}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}

