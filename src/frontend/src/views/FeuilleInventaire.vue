<template>
  <div class="feuille-inventaire">
    <div class="header">
      <button class="back-button" @click="goBack">←</button>
      <h1>{{ agenceName }} - {{ magasinName }}</h1>
    </div>

    <div v-if="loading" class="loading">
      Chargement des articles...
    </div>
    <div v-else-if="error" class="error">
      {{ error }}
    </div>
    <div v-else class="familles-list">
      <div v-for="famille in famillesGroupees" 
           :key="famille.famille" 
           class="famille">
        <div class="famille-header" @click="toggleFamille(famille)">
          <span class="toggle-icon">{{ famille.expanded ? '▼' : '▶' }}</span>
          {{ famille.famille }}
        </div>
        <div v-if="famille.expanded" class="sous-familles">
          <div v-for="sousFamille in famille.sousFamilles" 
               :key="sousFamille.sousFamille"
               class="sous-famille">
            <div class="sous-famille-header" @click="toggleSousFamille(sousFamille)">
              <span class="toggle-icon">{{ sousFamille.expanded ? '▼' : '▶' }}</span>
              <div class="sous-famille-content">
                <span>{{ sousFamille.sousFamille }}</span>
                <span class="articles-count">({{ sousFamille.articles.length }} articles)</span>
              </div>
            </div>
            <div v-if="sousFamille.expanded" class="articles-list">
              <table>
                <thead>
                  <tr>
                    <th>Code</th>
                    <th>Désignation</th>
                    <th>Quantité Terrain</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="article in sousFamille.articles" :key="article.code">
                    <td>{{ article.code }}</td>
                    <td>{{ article.designation }}</td>
                    <td>
                      <input 
                        type="number" 
                        v-model.number="article.quantiteTerrain"
                        min="0"
                        @change="handleQuantityChange(article)"
                      >
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { ref, onMounted, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { API_URL } from '../config'

export default {
  name: 'FeuilleInventaire',
  
  setup() {
    const route = useRoute()
    const router = useRouter()
    const loading = ref(true)
    const error = ref(null)
    const articles = ref([])
    const famillesGroupees = ref([])

    const agenceName = computed(() => route.params.agenceName || route.params.agence)
    const magasinName = computed(() => route.params.magasinName || route.params.magasin)

    const loadArticles = async () => {
      try {
        const encodedAgence = encodeURIComponent(route.params.agence)
        const encodedMagasin = encodeURIComponent(route.params.magasin)
        
        const response = await fetch(`${API_URL}/api/Agences/${encodedAgence}/magasins/${encodedMagasin}/articles`)
        if (!response.ok) throw new Error('Erreur lors du chargement des articles')
        
        articles.value = await response.json()

        console.log('Articles chargés:', articles.value);

        groupArticles()
      } catch (e) {
        error.value = e.message
        console.error('Erreur:', e)
      } finally {
        loading.value = false
      }
    }

    const groupArticles = () => {
      // Utiliser directement la structure du JSON reçu
      famillesGroupees.value = articles.value.map(famille => ({
        ...famille,
        expanded: false,
        sousFamilles: famille.sousFamilles.map(sf => ({
          ...sf,
          expanded: false
        }))
      }))
    }

    const toggleFamille = (famille) => {
      famille.expanded = !famille.expanded
    }

    const toggleSousFamille = (sousFamille) => {
      sousFamille.expanded = !sousFamille.expanded
    }

    const handleQuantityChange = (article) => {
      console.log('Quantité modifiée pour article:', article.code, 'Nouvelle valeur:', article.quantiteTerrain)
      // TODO: Appeler l'API pour sauvegarder la nouvelle quantité
    }

    const goBack = () => {
      router.back()
    }

    onMounted(loadArticles)

    return {
      agenceName,
      magasinName,
      loading,
      error,
      famillesGroupees,
      toggleFamille,
      toggleSousFamille,
      handleQuantityChange,
      goBack
    }
  }
}
</script>

<style scoped>
.feuille-inventaire {
  padding: 1rem;
}

.header {
  display: flex;
  align-items: center;
  margin-bottom: 1rem;
}

.back-button {
  background: none;
  border: none;
  font-size: 1.5rem;
  cursor: pointer;
  padding: 0.5rem;
  margin-right: 1rem;
}

.back-button:hover {
  color: #666;
}

h1 {
  margin: 0;
  font-size: 1.5rem;
}

.familles-list {
  margin-top: 1rem;
}

.famille {
  margin-bottom: 0.5rem;
  background-color: #f5f5f5;
  border-radius: 4px;
}

.famille-header {
  display: flex;
  align-items: center;
  padding: 1rem;
  cursor: pointer;
  user-select: none;
}

.famille-header:hover {
  background-color: #e0e0e0;
}

.toggle-icon {
  margin-right: 0.5rem;
  font-size: 0.8em;
}

.sous-familles {
  margin-left: 2rem;
  padding: 0.5rem 1rem;
}

.sous-famille {
  margin: 0.25rem 0;
  background-color: white;
  border-radius: 4px;
}

.sous-famille-header {
  display: flex;
  align-items: center;
  padding: 0.5rem;
  cursor: pointer;
}

.sous-famille-content {
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-grow: 1;
}

.articles-list {
  margin: 0.5rem;
  padding: 0.5rem;
  background-color: #f9f9f9;
  border-radius: 4px;
}

table {
  width: 100%;
  border-collapse: collapse;
}

th, td {
  padding: 0.5rem;
  text-align: left;
  border-bottom: 1px solid #ddd;
}

th {
  background-color: #f5f5f5;
  font-weight: 500;
}

input[type="number"] {
  width: 80px;
  padding: 0.3rem;
  border: 1px solid #ddd;
  border-radius: 4px;
}

input[type="number"]:focus {
  outline: none;
  border-color: #4CAF50;
  box-shadow: 0 0 0 2px rgba(76, 175, 80, 0.2);
}

.articles-count {
  font-size: 0.8em;
  color: #666;
  margin-left: 1rem;
}

.loading, .error {
  padding: 1rem;
  text-align: center;
}

.error {
  color: red;
}
</style>
