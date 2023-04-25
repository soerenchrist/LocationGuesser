import { createWebHashHistory, createRouter } from "vue-router";
import HomePage from "../components/home/HomePage.vue";

const routes = [
  {
    path: "/",
    name: "home",
    component: HomePage,
  },
  {
    path: "/game/:slug",
    name: "game",
    props: true,
    component: () => import("../components/game/GamePage.vue"),
  },
  {
    path: "/:pathMatch(.*)*",
    name: "NotFound",
    component: () => import("../components/NotFound.vue"),
  },
];

const router = createRouter({
  history: createWebHashHistory(),
  routes,
});

export default router;
