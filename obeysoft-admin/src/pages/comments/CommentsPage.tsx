import { useEffect, useState } from 'react';
import { commentApprove, commentDelete, commentsList, CommentRow } from '../../lib/api';
import { Button } from '../../components/ui/Button';
import { Dialog, DialogActions, DialogBody, DialogTitle } from '../../components/ui/Dialog';

export function CommentsPage() {
  const [rows, setRows] = useState<CommentRow[]>([]);
  const [loading, setLoading] = useState(true);
  const [err, setErr] = useState<string | null>(null);
  const [confirmId, setConfirmId] = useState<string | null>(null);

  async function load() {
    setLoading(true); setErr(null);
    try { setRows(await commentsList()); }
    catch (e:any) { setErr(e.message ?? 'Yüklenemedi'); }
    finally { setLoading(false); }
  }
  useEffect(() => { load(); }, []);

  return (
    <div className="space-y-4">
      <div className="text-xl font-semibold">Yorumlar</div>
      <div className="glass p-0 overflow-auto">
        <table className="table w-full">
          <thead>
            <tr><th>Yazı</th><th>Yazar</th><th>İçerik</th><th>Durum</th><th className="text-right">İşlemler</th></tr>
          </thead>
          <tbody>
            {loading && <tr><td colSpan={5} className="px-3 py-6 text-center text-white/70">Yükleniyor...</td></tr>}
            {err && !loading && <tr><td colSpan={5} className="px-3 py-6 text-center text-red-300">{err}</td></tr>}
            {!loading && !err && rows.length === 0 && <tr><td colSpan={5} className="px-3 py-6 text-center text-white/70">Kayıt yok.</td></tr>}
            {rows.map(r => (
              <tr key={r.id}>
                <td>{r.postTitle}</td>
                <td className="text-white/70">{r.author}</td>
                <td className="text-white/80">{r.content}</td>
                <td>{r.isApproved ? 'Onaylı' : 'Beklemede'}</td>
                <td className="text-right">
                  <div className="inline-flex gap-2">
                    {!r.isApproved && <Button onClick={async () => { await commentApprove(r.id); await load(); }}>Onayla</Button>}
                    <Button variant="danger" onClick={() => setConfirmId(r.id)}>Sil</Button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <Dialog open={!!confirmId} onClose={() => setConfirmId(null)}>
        <DialogTitle>Silme Onayı</DialogTitle>
        <DialogBody>Bu yorumu silmek istediğinizden emin misiniz?</DialogBody>
        <DialogActions>
          <Button variant="ghost" onClick={() => setConfirmId(null)}>İptal</Button>
          <Button variant="danger" onClick={async () => { if (!confirmId) return; await commentDelete(confirmId); setConfirmId(null); await load(); }}>Sil</Button>
        </DialogActions>
      </Dialog>
    </div>
  );
}


