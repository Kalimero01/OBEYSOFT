import { useEffect, useMemo, useState } from 'react';
import { postsList, postDelete, postTogglePublish, PostListItem, categoriesList, postCreate } from '../../lib/api';
import { Button } from '../../components/ui/Button';
import { Input } from '../../components/ui/Input';
import { Dialog, DialogActions, DialogBody, DialogTitle } from '../../components/ui/Dialog';
import { Edit2, Eye, EyeOff, Plus, Save, Search, Trash2 } from 'lucide-react';

export function PostsPage() {
  const [q, setQ] = useState('');
  const [page, setPage] = useState(1);
  const pageSize = 10;
  const [data, setData] = useState<{ items: PostListItem[]; page: number; pageSize: number; total: number } | null>(null);
  const [loading, setLoading] = useState(true);
  const [err, setErr] = useState<string | null>(null);
  const [confirmId, setConfirmId] = useState<string | null>(null);
  const [formOpen, setFormOpen] = useState(false);
  const [cats, setCats] = useState<{ id: string; name: string }[]>([]);
  const [form, setForm] = useState({ title: '', slug: '', categoryId: '', content: '', summary: '', isActive: true });

  async function load() {
    setLoading(true); setErr(null);
    try { setData(await postsList({ page, pageSize, search: q || undefined })); }
    catch (e:any) { setErr(e.message ?? 'Yüklenemedi'); }
    finally { setLoading(false); }
  }

  useEffect(() => { load(); }, [page]); // eslint-disable-line
  useEffect(() => { categoriesList().then(list => setCats(list.map(c => ({ id: (c as any).id, name: (c as any).name })))).catch(() => setCats([])); }, []);

  const totalPages = useMemo(() => data ? Math.max(1, Math.ceil(data.total / data.pageSize)) : 1, [data]);

  async function onDelete() {
    if (!confirmId) return;
    await postDelete(confirmId);
    setConfirmId(null);
    await load();
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center gap-2">
        <button onClick={() => { setForm({ title: '', slug: '', categoryId: '', content: '', summary: '', isActive: true }); setFormOpen(true); }} className="btn btn-primary"><Plus size={16} className="mr-1" /> Yeni Yazı</button>
        <div className="ml-auto flex items-center gap-2">
          <div className="relative">
            <Search className="absolute left-2 top-2.5 text-white/50" size={16} />
            <Input className="pl-8 w-64" placeholder="Başlıkta ara..." value={q} onChange={e => setQ(e.currentTarget.value)} />
          </div>
          <Button variant="ghost" onClick={() => { setPage(1); load(); }}>Ara</Button>
        </div>
      </div>

      <div className="glass p-0 overflow-auto">
        <table className="table w-full">
          <thead>
            <tr><th>Başlık</th><th>Kategori</th><th>Durum</th><th>Tarih</th><th className="text-right">İşlemler</th></tr>
          </thead>
          <tbody>
            {loading && <tr><td colSpan={5} className="px-3 py-6 text-center text-white/70">Yükleniyor...</td></tr>}
            {err && !loading && <tr><td colSpan={5} className="px-3 py-6 text-center text-red-300">{err}</td></tr>}
            {!loading && !err && (data?.items.length ?? 0) === 0 && <tr><td colSpan={5} className="px-3 py-6 text-center text-white/70">Kayıt yok.</td></tr>}
            {(data?.items ?? []).map(r => (
              <tr key={r.id}>
                <td>{r.title}</td>
                <td className="text-white/70">{r.categoryName}</td>
                <td>{r.isPublished ? <span className="badge badge-green">Yayında</span> : <span className="badge badge-yellow">Taslak</span>}</td>
                <td className="text-white/70">{new Date(r.createdAt).toLocaleString()}</td>
                <td className="text-right">
                  <div className="inline-flex gap-2">
                    <Link to={`/admin/posts/${r.id}`} className="btn btn-ghost"><Edit2 size={16} /></Link>
                    <Button variant="ghost" onClick={async () => { await postTogglePublish(r.id, !r.isPublished); await load(); }}>
                      {r.isPublished ? <EyeOff size={16} /> : <Eye size={16} />}
                    </Button>
                    <Button variant="danger" onClick={() => setConfirmId(r.id)}><Trash2 size={16} /></Button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <div className="flex items-center gap-2">
        <Button variant="ghost" disabled={page <= 1} onClick={() => setPage(p => Math.max(1, p - 1))}>Önceki</Button>
        <div className="text-sm text-white/70">Sayfa {page} / {totalPages}</div>
        <Button variant="ghost" disabled={page >= totalPages} onClick={() => setPage(p => p + 1)}>Sonraki</Button>
      </div>

      <Dialog open={!!confirmId} onClose={() => setConfirmId(null)}>
        <DialogTitle>Silme Onayı</DialogTitle>
        <DialogBody>Bu yazıyı silmek istediğinizden emin misiniz?</DialogBody>
        <DialogActions>
          <Button variant="ghost" onClick={() => setConfirmId(null)}>İptal</Button>
          <Button variant="danger" onClick={onDelete}>Sil</Button>
        </DialogActions>
      </Dialog>

      <Dialog open={formOpen} onClose={() => setFormOpen(false)}>
        <DialogTitle>Yeni Yazı</DialogTitle>
        <DialogBody>
          <div className="space-y-3">
            <div>
              <label className="block text-xs text-white/60 mb-1">Başlık</label>
              <Input value={form.title} onChange={e => setForm(f => ({ ...f, title: (e.target as HTMLInputElement).value }))} />
            </div>
            <div>
              <label className="block text-xs text-white/60 mb-1">Slug</label>
              <Input value={form.slug} placeholder="boşsa otomatik" onChange={e => setForm(f => ({ ...f, slug: (e.target as HTMLInputElement).value }))} />
            </div>
            <div>
              <label className="block text-xs text-white/60 mb-1">Kategori</label>
              <select className="input" value={form.categoryId} onChange={e => setForm(f => ({ ...f, categoryId: (e.target as HTMLSelectElement).value }))}>
                <option value="">Seçiniz</option>
                {cats.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
              </select>
            </div>
            <div>
              <label className="block text-xs text-white/60 mb-1">Özet</label>
              <Input value={form.summary} onChange={e => setForm(f => ({ ...f, summary: (e.target as HTMLInputElement).value }))} />
            </div>
            <div>
              <label className="block text-xs text-white/60 mb-1">İçerik</label>
              <textarea className="input h-40" value={form.content} onChange={e => setForm(f => ({ ...f, content: (e.target as HTMLTextAreaElement).value }))} />
            </div>
            <label className="inline-flex items-center gap-2">
              <input type="checkbox" checked={form.isActive} onChange={e => setForm(f => ({ ...f, isActive: (e.target as HTMLInputElement).checked }))} />
              Aktif
            </label>
          </div>
        </DialogBody>
        <DialogActions>
          <Button variant="ghost" onClick={() => setFormOpen(false)}>İptal</Button>
          <Button onClick={async () => {
            const body = {
              title: form.title,
              slug: form.slug || form.title.toLowerCase().replace(/\s+/g, '-'),
              categoryId: form.categoryId,
              content: form.content,
              summary: form.summary || null,
              isActive: form.isActive
            };
            await postCreate(body);
            setFormOpen(false);
            await load();
          }}><Save size={16} className="mr-1" />Kaydet</Button>
        </DialogActions>
      </Dialog>
    </div>
  );
}


