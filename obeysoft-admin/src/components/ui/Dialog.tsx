import { ReactNode, useEffect } from 'react';

export function Dialog({ open, onClose, children }:{ open:boolean; onClose:()=>void; children:ReactNode }) {
  useEffect(() => {
    function onKey(e: KeyboardEvent) { if (e.key === 'Escape') onClose(); }
    if (open) document.addEventListener('keydown', onKey);
    return () => document.removeEventListener('keydown', onKey);
  }, [open, onClose]);

  if (!open) return null;
  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center">
      <div className="absolute inset-0 bg-black/60" onClick={onClose} />
      <div className="relative w-full max-w-lg glass p-4">
        {children}
      </div>
    </div>
  );
}

export function DialogTitle({ children }:{ children:ReactNode }) {
  return <div className="text-lg font-semibold mb-2">{children}</div>;
}
export function DialogBody({ children }:{ children:ReactNode }) {
  return <div className="text-sm text-white/80">{children}</div>;
}
export function DialogActions({ children }:{ children:ReactNode }) {
  return <div className="mt-4 flex items-center justify-end gap-2">{children}</div>;
}


