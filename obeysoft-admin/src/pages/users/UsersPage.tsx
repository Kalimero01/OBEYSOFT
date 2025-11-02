import { useEffect, useState } from 'react';
import { userSetActive, usersList, UserRow } from '../../lib/api';

export function UsersPage() {
  const [rows, setRows] = useState<UserRow[]>([]);
  const [loading, setLoading] = useState(true);
  const [err, setErr] = useState<string | null>(null);

  async function load() {
    setLoading(true); setErr(null);
    try { setRows(await usersList()); }
    catch (e:any) { setErr(e.message ?? 'Yüklenemedi'); }
    finally { setLoading(false); }
  }
  useEffect(() => { load(); }, []);

  return (
    <div className="space-y-4">
      <div className="text-xl font-semibold">Kullanıcılar</div>
      <div className="glass p-0 overflow-auto">
        <table className="table w-full">
          <thead>
            <tr><th>Ad Soyad</th><th>E-posta</th><th>Rol</th><th>Aktif</th><th className="text-right">İşlemler</th></tr>
          </thead>
          <tbody>
            {loading && <tr><td colSpan={5} className="px-3 py-6 text-center text-white/70">Yükleniyor...</td></tr>}
            {err && !loading && <tr><td colSpan={5} className="px-3 py-6 text-center text-red-300">{err}</td></tr>}
            {!loading && !err && rows.length === 0 && <tr><td colSpan={5} className="px-3 py-6 text-center text-white/70">Kayıt yok.</td></tr>}
            {rows.map(r => (
              <tr key={r.id}>
                <td>{r.displayName}</td>
                <td className="text-white/70">{r.email}</td>
                <td>{r.role}</td>
                <td>{r.isActive ? 'Evet' : 'Hayır'}</td>
                <td className="text-right">
                  <button className="btn btn-ghost" onClick={async () => { await userSetActive(r.id, !r.isActive); await load(); }}>
                    {r.isActive ? 'Pasifleştir' : 'Aktifleştir'}
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}


