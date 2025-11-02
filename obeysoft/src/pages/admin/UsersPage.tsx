import { useEffect, useState } from "react";

import { Badge } from "../../components/ui/Badge";
import { Button } from "../../components/ui/Button";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow
} from "../../components/ui/Table";
import { adminUserSetActive, adminUsersList, type UserRow } from "../../lib/api";
import { useAuthStore } from "../../store/auth";

export function UsersPage() {
  const auth = useAuthStore((state) => state.user);
  const [rows, setRows] = useState<UserRow[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const load = async () => {
    setLoading(true);
    setError(null);
    try {
      const list = await adminUsersList();
      setRows(list);
    } catch (err: any) {
      setError(err?.message ?? "Kullanıcılar alınamadı");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (auth?.role === "Admin") {
      load();
    }
  }, [auth?.role]);

  if (auth?.role !== "Admin") {
    return (
      <div className="rounded-3xl border border-border/70 bg-card/80 p-6 text-sm text-muted-foreground">
        Bu alanı görüntülemek için yönetici yetkisi gerekiyor.
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold">Kullanıcılar</h1>
        <p className="text-sm text-muted-foreground">Sistem kullanıcılarını ve rollerini yönetin.</p>
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
              <TableHead>Ad Soyad</TableHead>
              <TableHead>E-posta</TableHead>
              <TableHead>Rol</TableHead>
              <TableHead>Durum</TableHead>
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
            {!loading && rows.length === 0 && (
              <TableRow>
                <TableCell colSpan={5} className="py-6 text-center text-sm text-muted-foreground">
                  Kayıt bulunamadı.
                </TableCell>
              </TableRow>
            )}
            {rows.map((row) => (
              <TableRow key={row.id}>
                <TableCell className="font-medium text-foreground">{row.displayName}</TableCell>
                <TableCell>{row.email}</TableCell>
                <TableCell>
                  <Badge variant={row.role === "Admin" ? "success" : "outline"}>{row.role}</Badge>
                </TableCell>
                <TableCell>
                  <Badge variant={row.isActive ? "success" : "warning"}>
                    {row.isActive ? "Aktif" : "Pasif"}
                  </Badge>
                </TableCell>
                <TableCell className="text-right">
                  <Button
                    variant="ghost"
                    size="sm"
                    onClick={async () => {
                      await adminUserSetActive(row.id, !row.isActive);
                      await load();
                    }}
                  >
                    {row.isActive ? "Pasifleştir" : "Aktifleştir"}
                  </Button>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>
    </div>
  );
}


