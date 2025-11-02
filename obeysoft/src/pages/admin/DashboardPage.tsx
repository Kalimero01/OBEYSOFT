import { motion } from "framer-motion";
import { FileText, Folder, Users } from "lucide-react";
import { useEffect, useState } from "react";

import {
  getDashboardSnapshot,
  type PostListItem
} from "../../lib/api";
import { Badge } from "../../components/ui/Badge";
import { Card, CardContent } from "../../components/ui/Card";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow
} from "../../components/ui/Table";

type DashboardStats = {
  totalPosts: number;
  publishedPosts: number;
  totalCategories: number;
  totalUsers: number;
  latestPosts: PostListItem[];
};

export function DashboardPage() {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [stats, setStats] = useState<DashboardStats | null>(null);

  useEffect(() => {
    (async () => {
      setLoading(true);
      setError(null);
      try {
        const snapshot = await getDashboardSnapshot();
        setStats(snapshot);
      } catch (err: any) {
        setError(err?.message ?? "Veriler alınamadı");
      } finally {
        setLoading(false);
      }
    })();
  }, []);

  if (loading) return <div className="text-sm text-muted-foreground">Veriler yükleniyor...</div>;
  if (error) return <div className="text-sm text-destructive">{error}</div>;
  if (!stats) return <div className="text-sm text-muted-foreground">Veri bulunamadı.</div>;

  return (
    <div className="space-y-6">
      <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        <StatCard icon={<FileText />} label="Toplam Yazı" value={stats.totalPosts} />
        <StatCard icon={<FileText />} label="Yayında Olan" value={stats.publishedPosts} />
        <StatCard icon={<Folder />} label="Kategori" value={stats.totalCategories} />
        <StatCard icon={<Users />} label="Kullanıcı" value={stats.totalUsers} />
      </div>

      <Card className="border border-border/70 bg-card/80">
        <CardContent className="space-y-4 p-6">
          <div className="flex items-center justify-between">
            <h2 className="text-lg font-semibold text-foreground">Son Yazılar</h2>
            <Badge variant="outline">{stats.latestPosts.length}</Badge>
          </div>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Başlık</TableHead>
                <TableHead>Kategori</TableHead>
                <TableHead>Yayın Durumu</TableHead>
                <TableHead className="text-right">Tarih</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {stats.latestPosts.map((post) => (
                <TableRow key={post.id}>
                  <TableCell className="font-medium text-foreground">{post.title}</TableCell>
                  <TableCell>{post.categoryName}</TableCell>
                  <TableCell>
                    <Badge variant={post.isPublished ? "success" : "warning"}>
                      {post.isPublished ? "Yayında" : "Taslak"}
                    </Badge>
                  </TableCell>
                  <TableCell className="text-right">
                    {new Date(post.createdAt).toLocaleDateString("tr-TR")}
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </CardContent>
      </Card>
    </div>
  );
}

function StatCard({ icon, label, value }: { icon: React.ReactNode; label: string; value: number }) {
  return (
    <motion.div
      initial={{ opacity: 0, y: 12 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.25 }}
    >
      <Card className="border border-border/70 bg-card/80">
        <CardContent className="flex items-center gap-4 p-5">
          <span className="rounded-2xl bg-primary/15 p-3 text-primary">{icon}</span>
          <div>
            <p className="text-xs uppercase tracking-wide text-muted-foreground">{label}</p>
            <div className="text-2xl font-semibold text-foreground">{value}</div>
          </div>
        </CardContent>
      </Card>
    </motion.div>
  );
}


