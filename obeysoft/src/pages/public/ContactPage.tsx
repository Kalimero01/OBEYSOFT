import { useState } from "react";

import { Button } from "../../components/ui/Button";
import { Card, CardContent } from "../../components/ui/Card";
import { Input } from "../../components/ui/Input";
import { Label } from "../../components/ui/Label";
import { Textarea } from "../../components/ui/Textarea";

export function ContactPage() {
  const [busy, setBusy] = useState(false);
  const [success, setSuccess] = useState(false);

  const handleSubmit = (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setBusy(true);
    setTimeout(() => {
      setBusy(false);
      setSuccess(true);
    }, 700);
  };

  return (
    <div className="mx-auto max-w-xl space-y-6">
      <div>
        <h1 className="text-3xl font-semibold">İletişim</h1>
        <p className="text-sm text-muted-foreground">Projeleriniz ve sorularınız için bize ulaşın.</p>
      </div>

      <Card className="border border-border/70 bg-card/80">
        <CardContent className="space-y-5 p-6">
          {success ? (
            <div className="rounded-2xl border border-primary/30 bg-primary/10 px-4 py-5 text-sm text-primary">
              Mesajınız için teşekkürler! En kısa sürede dönüş yapacağız.
            </div>
          ) : (
            <form className="space-y-4" onSubmit={handleSubmit}>
              <div className="space-y-2">
                <Label htmlFor="name">Ad Soyad</Label>
                <Input id="name" name="name" placeholder="Adınız" required disabled={busy} />
              </div>
              <div className="space-y-2">
                <Label htmlFor="email">E-posta</Label>
                <Input id="email" type="email" name="email" placeholder="ornek@obeysoft.dev" required disabled={busy} />
              </div>
              <div className="space-y-2">
                <Label htmlFor="message">Mesajınız</Label>
                <Textarea id="message" name="message" placeholder="Nasıl yardımcı olabiliriz?" required disabled={busy} />
              </div>
              <Button type="submit" disabled={busy} className="w-full">
                {busy ? "Gönderiliyor..." : "Gönder"}
              </Button>
            </form>
          )}
        </CardContent>
      </Card>
    </div>
  );
}


