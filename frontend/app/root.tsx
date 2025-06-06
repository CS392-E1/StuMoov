import { Links, Meta, Outlet, Scripts, ScrollRestoration } from "react-router";
import { Toaster } from "sonner";

import React from "react";

export function Layout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="en">
      <head>
        <meta charSet="UTF-8" />
        <meta name="viewport" content="width=device-width, initial-scale=1.0" />
        <title>StuMoov</title>
        <Meta />
        <Links />
      </head>
      <body>
        {children}
        <ScrollRestoration />
        <Scripts />
        <Toaster />
      </body>
    </html>
  );
}

export default function Root() {
  return <Outlet />;
}
