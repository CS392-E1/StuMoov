import {
  type RouteConfig,
  index,
  layout,
  route,
} from "@react-router/dev/routes";

export default [
  layout("./components/layout/RootLayout.tsx", [
    index("./pages/Home.tsx"),
    route("login", "./pages/auth/Login.tsx"),
    route("register", "./pages/auth/Register.tsx"),

    route("listings", "./pages/listings/Listings.tsx"),
    route("add-listing", "./pages/listings/AddListing.tsx"), 
  ]),
] satisfies RouteConfig;
