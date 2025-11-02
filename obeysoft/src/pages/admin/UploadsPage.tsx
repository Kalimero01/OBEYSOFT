import { useRef, useState } from "react";

import { Button } from "../../components/ui/Button";
import { Card, CardContent } from "../../components/ui/Card";
import { adminUpload } from "../../lib/api";

export function UploadsPage() {
  const inputRef = useRef<HTMLInputElement | null>(null);
  const [selected, setSelected] = useState<File | null>(null);
  const [uploading, setUploading] = useState(false);
  const [url, setUrl] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  const handleUpload = async () => {
    if (!selected) return;
    setUploading(true);
    setError(null);
    try {
      const result = await adminUpload(selected);
      setUrl(result.url);
      setSelected(null);
      if (inputRef.current) inputRef.current.value = "";
    } catch (err: any) {
      setError(err?.message ?? "Yükleme başarısız oldu");
    } finally {
      setUploading(false);
    }
  };

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold">Dosya Yükleme</h1>
        <p className="text-sm text-muted-foreground">Yüklenen dosyalar `wwwroot/uploads` altında saklanır ve URL döner.</p>
      </div>

      {error && (
        <div className="rounded-2xl border border-destructive/50 bg-destructive/10 p-3 text-sm text-destructive">
          {error}
        </div>
      )}

      <Card className="border border-border/70 bg-card/80">
        <CardContent className="space-y-4 p-6">
          <input
            ref={inputRef}
            type="file"
            onChange={(event) => {
              const file = event.currentTarget.files?.[0];
              setSelected(file ?? null);
              setUrl(null);
            }}
            className="block w-full text-sm text-muted-foreground file:mr-4 file:rounded-full file:border-0 file:bg-primary/15 file:px-4 file:py-2 file:text-sm file:font-semibold file:text-primary hover:file:bg-primary/20"
          />
          <div className="flex items-center justify-between">
            <div className="text-sm text-muted-foreground">
              {selected ? `${selected.name} (${Math.round(selected.size / 1024)} KB)` : "Henüz dosya seçilmedi."}
            </div>
            <Button disabled={!selected || uploading} onClick={handleUpload}>
              {uploading ? "Yükleniyor..." : "Yükle"}
            </Button>
          </div>
          {url && (
            <div className="rounded-2xl border border-primary/30 bg-primary/10 px-4 py-3 text-sm text-primary">
              Dosya yüklendi. URL: <a href={url} target="_blank" rel="noreferrer" className="underline">{url}</a>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}

