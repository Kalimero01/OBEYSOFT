import { useEffect, useMemo, useState } from 'react';
import { categoriesList, categoryDelete, categoryUpsert, Category } from '../../lib/api';
import { Button } from '../../components/ui/Button';
import { Input } from '../../components/ui/Input';
import { Dialog, DialogActions, DialogBody, DialogTitle } from '../../components/ui/Dialog';
import { Plus, Save, Trash2 } from 'lucide-react';

export function CategoriesPage() {
  const [rows, setRows] = useState<Category[]>([]);
  const [loading, setLoading] = useState(true);
  const [err, setErr] = useState<string | null>(null);
  const [formOpen, setFormOpen] = useState(false);
  const [selectedId, setSelectedId] = useState<string | null>(null);
  const selected = useMemo(() => rows.find(r => r.id === selectedId) ?? null, [rows, selectedId]);

  const [form, setForm] = useState<Omit<Category,'id'> & { description?: string | null }>({
    name: '', slug: '', description: '', parentId: null, displayOrder: 1, isActive: true
  });

  function resetForm() {
    setSelectedId(null);
    setForm({ name: '', slug: '', description: '', parentId: null, displayOrder: 1, isActive: true });
  }

  async function load() {
    setLoading(true); setErr(null);
    try { setRows(await categoriesList()); }
    catch (e:any) { setErr(e.message ?? 'Yüklenemedi'); }
    finally { setLoading(false); }
  }

  useEffect(() => { load(); }, []);
  useEffect(() => {
    if (!selected) return;
    setForm({
      name: selected.name,
      slug: selected.slug,
      description: '',
      parentId: selected.parentId ?? null,
      displayOrder: selected.displayOrder,
      isActive: selected.isActive
    });
  }, [selectedId]);

  async function save() {
    await categoryUpsert(selectedId, {
      ...form,
      slug: form.slug || form.name.toLowerCase().replace(/\s+/g, '-')
    });
    setFormOpen(false);
    resetForm();
    await load();
  }

  async function remove(id: string) {
    await categoryDelete(id);
    if (id === selectedId) resetForm();
    await load();
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center">
        <div className="text-xl font-semibold">Kategoriler</div>
        <div className="ml-auto">
          <Button onClick={() => { resetForm(); setFormOpen(true); }}><Plus size={16} className="mr-1" />Yeni Kategori</Button>
        </div>
      </div>

      <div className="glass p-0 overflow-auto">
        <table className="table w-full">
          <thead>
            <tr><th>Ad</th><th>Slug</th><th>Üst</th><th>Sıra</th><th>Aktif</th><th className="text-right">İşlemler</th></tr>
          </thead>
          <tbody>
            {loading && <tr><td colSpan={6} className="px-3 py-6 text-center text-white/70">Yükleniyor...</td></tr>}
            {err && !loading && <tr><td colSpan={6} className="px-3 py-6 text-center text-red-300">{err}</td></tr>}
            {!loading && !err && rows.length === 0 && <tr><td colSpan={6} className="px-3 py-6 text-center text-white/70">Kayıt yok.</td></tr>}
            {rows.map(r => (
              <tr key={r.id}>
                <td>{r.name}</td>
                <td className="text-white/70">{r.slug}</td>
                <td className="text-white/70">{rows.find(x => x.id === r.parentId)?.name ?? '-'}</td>
                <td className="text-white/70">{r.displayOrder}</td>
                <td>{r.isActive ? 'Evet' : 'Hayır'}</td>
                <td className="text-right">
                  <div className="inline-flex gap-2">
                    <Button variant="ghost" onClick={() => { setSelectedId(r.id); setFormOpen(true); }}>Düzenle</Button>
                    <Button variant="danger" onClick={() => remove(r.id)}><Trash2 size={16} /></Button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <Dialog open={formOpen} onClose={() => setFormOpen(false)}>
        <DialogTitle>{selectedId ? 'Kategori Düzenle' : 'Yeni Kategori'}</DialogTitle>
        <DialogBody>
          <div className="space-y-3">
            <div>
              <label className="block text-xs text-white/60 mb-1">Ad</label>
              <Input value={form.name} onChange={e => { const v = (e.target as HTMLInputElement).value; setForm(f => ({ ...f, name: v })); }} />
            </div>
            <div>
              <label className="block text-xs text-white/60 mb-1">Slug</label>
              <Input value={form.slug} placeholder="boşsa otomatik oluşturulur" onChange={e => { const v = (e.target as HTMLInputElement).value; setForm(f => ({ ...f, slug: v })); }} />
            </div>
            <div className="grid grid-cols-2 gap-3">
              <div>
                <label className="block text-xs text-white/60 mb-1">Üst Kategori</label>
                <select
                  className="input"
                  value={form.parentId ?? ''}
                  onChange={e => { const v = (e.target as HTMLSelectElement).value; setForm(f => ({ ...f, parentId: v || null })); }}
                >
                  <option value="">(Yok)</option>
                  {rows.filter(x => x.id !== selectedId).map(x => <option key={x.id} value={x.id}>{x.name}</option>)}
                </select>
              </div>
              <div>
                <label className="block text-xs text-white/60 mb-1">Sıra</label>
                <Input type="number" value={String(form.displayOrder)} onChange={e => { const v = (e.target as HTMLInputElement).value; setForm(f => ({ ...f, displayOrder: Number(v || '0') })); }} />
              </div>
            </div>
            <label className="inline-flex items-center gap-2">
              <input type="checkbox" checked={!!form.isActive} onChange={e => { const v = (e.target as HTMLInputElement).checked; setForm(f => ({ ...f, isActive: v })); }} />
              Aktif
            </label>
          </div>
        </DialogBody>
        <DialogActions>
          <Button variant="ghost" onClick={() => setFormOpen(false)}>İptal</Button>
          <Button onClick={save}><Save size={16} className="mr-1" />Kaydet</Button>
        </DialogActions>
      </Dialog>
    </div>
  );
}


