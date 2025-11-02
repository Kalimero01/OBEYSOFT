import { useEffect, useState } from "react";

import {
  adminCommentApprove,
  adminCommentDelete,
  adminCommentsList,
  type CommentRow
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
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow
} from "../../components/ui/Table";

export function CommentsPage() {
  const [rows, setRows] = useState<CommentRow[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [confirmId, setConfirmId] = useState<string | null>(null);

  const load = async () => {
    setLoading(true);
    setError(null);
    try {
      const list = await adminCommentsList();
      setRows(list);
    } catch (err: any) {
      setError(err?.message ?? "Yorumlar alınamadı");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load();
  }, []);

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold">Yorumlar</h1>
        <p className="text-sm text-muted-foreground">Yayınlanan içeriklere gelen etkileşimleri yönetin.</p>
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
              <TableHead>Yazı</TableHead>
              <TableHead>Yazar</TableHead>
              <TableHead>İçerik</TableHead>
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
                  Henüz yorum bulunmuyor.
                </TableCell>
              </TableRow>
            )}
            {rows.map((row) => (
              <TableRow key={row.id}>
                <TableCell className="font-medium text-foreground">{row.postTitle}</TableCell>
                <TableCell>{row.author}</TableCell>
                <TableCell>{row.content}</TableCell>
                <TableCell>
                  <Badge variant={row.isApproved ? "success" : "warning"}>
                    {row.isApproved ? "Onaylı" : "Beklemede"}
                  </Badge>
                </TableCell>
                <TableCell className="text-right">
                  <div className="inline-flex items-center gap-2">
                    {!row.isApproved && (
                      <Button
                        variant="ghost"
                        size="sm"
                        onClick={async () => {
                          await adminCommentApprove(row.id);
                          await load();
                        }}
                      >
                        Onayla
                      </Button>
                    )}
                    <Button
                      variant="ghost"
                      size="sm"
                      className="text-destructive hover:text-destructive"
                      onClick={() => setConfirmId(row.id)}
                    >
                      Sil
                    </Button>
                  </div>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>

      <Dialog open={!!confirmId} onOpenChange={(open) => !open && setConfirmId(null)}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Yorumu silmek üzeresiniz</DialogTitle>
          </DialogHeader>
          <p className="text-sm text-muted-foreground">Yorum kalıcı olarak silinecek. Devam etmek istediğinize emin misiniz?</p>
          <DialogFooter>
            <Button variant="ghost" onClick={() => setConfirmId(null)}>
              İptal
            </Button>
            <Button
              variant="destructive"
              onClick={async () => {
                if (!confirmId) return;
                await adminCommentDelete(confirmId);
                setConfirmId(null);
                await load();
              }}
            >
              Sil
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}


