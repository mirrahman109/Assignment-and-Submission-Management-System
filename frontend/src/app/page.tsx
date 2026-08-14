// middleware.ts redirects every request to "/" before it reaches here (to /login or the
// user's role dashboard); this only renders in the brief window before that redirect lands.
export default function RootPage() {
  return null;
}
