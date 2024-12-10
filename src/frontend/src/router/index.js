import { createRouter, createWebHistory } from 'vue-router'
import AgencesList from '../views/AgencesList.vue'
import MagasinsList from '../views/MagasinsList.vue'

const routes = [
  {
    path: '/',
    name: 'agences',
    component: AgencesList
  },
  {
    path: '/agences/:agence/magasins',
    name: 'magasins',
    component: () => import('../views/MagasinsList.vue'),
    props: true
  },
  {
    path: '/agences/:agence/magasins/:magasin/articles',
    name: 'feuille-inventaire',
    component: () => import('../views/FeuilleInventaire.vue'),
    props: true
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

export default router
