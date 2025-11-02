import { useState } from 'react';
import { loginRequest } from '../../lib/api';

export function LoginPage() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [busy, setBusy] = useState(false);
  const [err, setErr] = useState<string | null>(null);

  async function submit(e: React.FormEvent) {
    e.preventDefault();
    setBusy(true); setErr(null);
    try {
      const { token } = await loginRequest(email, password);
      localStorage.setItem('access_token', token);
      window.location.href = '/admin/dashboard';
    } catch (e:any) {
      setErr(e.message ?? 'Giriş başarısız');
    } finally {
      setBusy(false);
    }
  }

  return (
    <div className="min-h-screen grid place-items-center p-4 bg-[#0b0f15] text-white">
      <form onSubmit={submit} className="glass p-6 w-full max-w-sm space-y-4">
        <div className="text-lg font-semibold">Admin Giriş</div>
        {err && <div className="text-sm text-red-300">{err}</div>}
        <div>
          <label className="block text-xs text-white/60 mb-1">E-posta</label>
          <input className="input" type="email" value={email} onChange={e => setEmail(e.currentTarget.value)} required />
        </div>
        <div>
          <label className="block text-xs text-white/60 mb-1">Parola</label>
          <input className="input" type="password" value={password} onChange={e => setPassword(e.currentTarget.value)} required />
        </div>
        <button disabled={busy} className="btn btn-primary w-full">{busy ? 'Giriş yapılıyor...' : 'Giriş Yap'}</button>
      </form>
    </div>
  );
}


