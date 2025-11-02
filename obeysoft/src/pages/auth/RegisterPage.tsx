import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";

import { Button } from "../../components/ui/Button";
import { Card, CardContent } from "../../components/ui/Card";
import { Input } from "../../components/ui/Input";
import { Label } from "../../components/ui/Label";
import { useAuthStore } from "../../store/auth";

export function RegisterPage() {
  const navigate = useNavigate();
  const register = useAuthStore((state) => state.register);
  const [email, setEmail] = useState("");
  const [displayName, setDisplayName] = useState("");
  const [password, setPassword] = useState("");
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const submit = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setBusy(true);
    setError(null);
    try {
      await register(email, displayName, password);
      navigate("/", { replace: true });
    } catch (err: any) {
      setError(err?.message ?? "Kayıt başarısız");
    } finally {
      setBusy(false);
    }
  };

  return (
    <div className="flex min-h-screen items-center justify-center bg-background">
      <Card className="w-full max-w-sm border border-border/70 bg-card/80">
        <CardContent className="space-y-5 p-6">
          <div>
            <h1 className="text-2xl font-semibold text-foreground">Hesap Oluştur</h1>
            <p className="text-sm text-muted-foreground">Hemen kayıt olun ve blog yazılarını okuyun.</p>
          </div>
          {error && (
            <div className="rounded-2xl border border-destructive/50 bg-destructive/10 px-3 py-2 text-sm text-destructive">
              {error}
            </div>
          )}
          <form className="space-y-4" onSubmit={submit}>
            <div className="space-y-2">
              <Label htmlFor="displayName">Ad Soyad</Label>
              <Input
                id="displayName"
                type="text"
                value={displayName}
                onChange={(event) => setDisplayName(event.currentTarget.value)}
                required
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="email">E-posta</Label>
              <Input
                id="email"
                type="email"
                autoComplete="email"
                value={email}
                onChange={(event) => setEmail(event.currentTarget.value)}
                required
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="password">Parola</Label>
              <Input
                id="password"
                type="password"
                autoComplete="new-password"
                value={password}
                onChange={(event) => setPassword(event.currentTarget.value)}
                required
              />
            </div>
            <Button type="submit" disabled={busy} className="w-full">
              {busy ? "Kaydediliyor..." : "Kayıt Ol"}
            </Button>
          </form>
          <div className="text-center text-sm text-muted-foreground">
            Zaten hesabınız var mı?{" "}
            <Link to="/login" className="font-medium text-primary hover:underline">
              Giriş yapın
            </Link>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}

