import { useEffect, useState } from 'react';
import { FileText, Folder, Users } from 'lucide-react';

type Stats = { totalPosts: number; publishedPosts: number; totalCategories: number; totalUsers: number };
type Row = { id: string; title: string; category: string; createdAt: string };

export function DashboardPage() {
  const [loading, setLoading] = useState(true);
  const [stats, setStats] = useState<Stats | null>(null);
  const [rows, setRows] = useState<Row[]>([]);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let mounted = true;
    (async () => {
      try {
        await new Promise(r => setTimeout(r, 300));
        if (!mounted) return;
        setStats({ totalPosts: 24, publishedPosts: 18, totalCategories: 7, totalUsers: 5 });
        setRows([
          { id: '1', title: '.NET 8 ve EF Core', category: 'ASP.NET Core', createdAt: new Date().toISOString() },
          { id: '2', title: 'Render Deploy', category: '.NET', createdAt: new Date().toISOString() },
          { id: '3', title: 'Next.js 14', category: 'Frontend', createdAt: new Date().toISOString() },
          { id: '4', title: 'DDD Giriş', category: 'Mimari', createdAt: new Date().toISOString() },
          { id: '5', title: 'PostgreSQL İpuçları', category: 'DB', createdAt: new Date().toISOString() }
        ]);
      } catch (e:any) {
        setError(e.message ?? 'Yüklenemedi');
      } finally {
        setLoading(false);
      }
    })();
    return () => { mounted = false; };
  }, []);

  if (loading) return <div>Yükleniyor...</div>;
  if (error) return <div className="text-red-300">{error}</div>;
  if (!stats) return <div>Veri yok.</div>;

  return (
    <div className="space-y-6">
      <div className="grid sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <StatCard icon={<FileText />} label="Toplam Yazı" value={stats.totalPosts} />
        <StatCard icon={<FileText />} label="Yayında Olanlar" value={stats.publishedPosts} />
        <StatCard icon={<Folder />} label="Kategori Sayısı" value={stats.totalCategories} />
        <StatCard icon={<Users />} label="Kullanıcı Sayısı" value={stats.totalUsers} />
      </div>

      <div className="glass p-4">
        <div className="font-semibold mb-3">Son Yazılar</div>
        <table className="table w-full">
          <thead>
            <tr><th>Başlık</th><th>Kategori</th><th>Tarih</th></tr>
          </thead>
          <tbody>
            {rows.map(r => (
              <tr key={r.id}>
                <td>{r.title}</td>
                <td className="text-white/70">{r.category}</td>
                <td className="text-white/70">{new Date(r.createdAt).toLocaleString()}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}

function StatCard({ icon, label, value }:{ icon:React.ReactNode; label:string; value:number }) {
  return (
    <div className="glass p-4 flex items-center gap-3">
      <div className="w-10 h-10 rounded bg-white/10 grid place-items-center">{icon}</div>
      <div>
        <div className="text-sm text-white/70">{label}</div>
        <div className="text-xl font-semibold">{value}</div>
      </div>
    </div>
  );
}


