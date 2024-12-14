<template>
  <div class="feuille-inventaire">
    <div class="header">
      <button class="back-button" @click="goBack">←</button>
      <div class="title">{{ agenceName }} - {{ magasinName }}</div>
      <div class="expand-buttons">
        <button class="action-button" @click="expandAll">Déplier tout</button>
        <button class="action-button" @click="collapseAll">Replier tout</button>
      </div>
    </div>

    <div v-if="loading" class="loading">
      Chargement...
    </div>
    <div v-else-if="error" class="error">
      {{ error }}
    </div>
    <div v-else>
      <div class="familles-list">
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
                        <div class="quantity-controls">
                          <input 
                            type="number" 
                            v-model.number="article.quantiteTerrain"
                            min="0"
                            @change="handleQuantityChange(article)"
                          >
                          <div class="quantity-shortcuts">
                            <button class="shortcut-btn" @click="incrementQuantity(article, 1)">+1</button>
                            <button class="shortcut-btn" @click="incrementQuantity(article, 5)">+5</button>
                            <button class="shortcut-btn" @click="incrementQuantity(article, 10)">+10</button>
                          </div>
                        </div>
                      </td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </div>
          </div>
        </div>
      </div>
      <div class="validation-section">
        <div class="connection-status" :class="{ 'offline': !isOnline }">
          {{ isOnline ? 'Connecté' : 'Mode hors-ligne' }}
        </div>
        <div class="pending-changes" v-if="hasPendingChanges">
          {{ pendingChangesCount }} modifications en attente
        </div>
        <button 
          class="validate-button" 
          :disabled="!isOnline || !hasPendingChanges"
          @click="validateChanges"
        >
          Valider la feuille
        </button>
      </div>
    </div>
  </div>
</template>

<script>
import { ref, computed, onMounted, onUnmounted } from 'vue'
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
    const isOnline = ref(navigator.onLine)
    
    // Clé unique pour le localStorage basée sur l'agence et le magasin
    const storageKey = computed(() => 
      `inventory_changes_${route.params.agence}_${route.params.magasin}`
    )

    // Charger les modifications en attente du localStorage
    const loadPendingChanges = () => {
      const stored = localStorage.getItem(storageKey.value)
      return stored ? JSON.parse(stored) : {}
    }

    const pendingChanges = ref(loadPendingChanges())

    const hasPendingChanges = computed(() => Object.keys(pendingChanges.value).length > 0)
    const pendingChangesCount = computed(() => Object.keys(pendingChanges.value).length)

    // Gestionnaires de la connectivité
    const handleOnline = () => {
      isOnline.value = true
    }

    const handleOffline = () => {
      isOnline.value = false
    }

    onMounted(() => {
      window.addEventListener('online', handleOnline)
      window.addEventListener('offline', handleOffline)
      loadArticles()
    })

    onUnmounted(() => {
      window.removeEventListener('online', handleOnline)
      window.removeEventListener('offline', handleOffline)
    })

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

        // Appliquer les modifications en attente aux articles chargés
        const stored = loadPendingChanges()
        articles.value.forEach(famille => {
          famille.sousFamilles.forEach(sf => {
            sf.articles.forEach(article => {
              if (stored[article.code]) {
                article.quantiteTerrain = stored[article.code].quantiteTerrain
              }
            })
          })
        })

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

    const expandAll = () => {
      famillesGroupees.value.forEach(famille => {
        famille.expanded = true
        famille.sousFamilles.forEach(sf => {
          sf.expanded = true
        })
      })
    }

    const collapseAll = () => {
      famillesGroupees.value.forEach(famille => {
        famille.expanded = false
        famille.sousFamilles.forEach(sf => {
          sf.expanded = false
        })
      })
    }

    const incrementQuantity = (article, amount) => {
      article.quantiteTerrain = (article.quantiteTerrain || 0) + amount
      handleQuantityChange(article)
    }

    const handleQuantityChange = (article) => {
      // Stocker la modification dans le localStorage
      pendingChanges.value[article.code] = {
        codeArticle: article.code,
        quantiteTerrain: article.quantiteTerrain
      }
      localStorage.setItem(storageKey.value, JSON.stringify(pendingChanges.value))
    }

    const validateChanges = async () => {
      if (!isOnline.value || !hasPendingChanges.value) return

      try {
        const changes = Object.values(pendingChanges.value)
        const response = await fetch(`${API_URL}/api/Agences/${encodeURIComponent(route.params.agence)}/magasins/${encodeURIComponent(route.params.magasin)}/articles`, {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify(changes)
        })

        if (!response.ok) throw new Error('Erreur lors de la validation des modifications')

        // Vider le localStorage après une validation réussie
        localStorage.removeItem(storageKey.value)
        pendingChanges.value = {}

        // Recharger les articles pour avoir les dernières valeurs
        await loadArticles()
      } catch (e) {
        error.value = e.message
      }
    }

    const goBack = () => {
      router.back()
    }

    return {
      agenceName,
      magasinName,
      loading,
      error,
      famillesGroupees,
      toggleFamille,
      toggleSousFamille,
      handleQuantityChange,
      incrementQuantity,
      expandAll,
      collapseAll,
      goBack,
      isOnline,
      hasPendingChanges,
      pendingChangesCount,
      validateChanges
    }
  }
}
</script>

<style scoped>
.container {
  padding: 0.5rem;
}

.header {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  margin-bottom: 0.5rem;
  justify-content: space-between;
  font-size: 0.9em;
}

.title {
  margin: 0;
  flex-grow: 1;
  font-weight: 500;
  color: #444;
}

.back-button {
  padding: 0.15rem 0.35rem;
  font-size: 0.9em;
  background-color: #f5f5f5;
  border: 1px solid #ddd;
  border-radius: 3px;
  cursor: pointer;
  color: #666;
}

.expand-buttons {
  display: flex;
  gap: 0.25rem;
}

.action-button {
  padding: 0.25rem 0.5rem;
  background-color: #f5f5f5;
  border: 1px solid #ddd;
  border-radius: 3px;
  cursor: pointer;
  font-size: 0.85em;
  color: #666;
}

.action-button:hover {
  background-color: #e8e8e8;
}

.familles-list {
  border: 1px solid #ddd;
  border-radius: 4px;
}

.famille {
  border-bottom: 1px solid #eee;
}

.famille:last-child {
  border-bottom: none;
}

.famille-header {
  padding: 0.25rem 0.5rem;
  cursor: pointer;
  display: flex;
  align-items: center;
  background-color: #f8f8f8;
  font-weight: 500;
}

.toggle-icon {
  margin-right: 0.5rem;
  font-size: 0.8em;
  width: 12px;
  display: inline-block;
}

.sous-famille {
  margin: 0;
  border-bottom: 1px solid #eee;
}

.sous-famille:last-child {
  border-bottom: none;
}

.sous-famille-header {
  display: flex;
  align-items: center;
  padding: 0.25rem 0.5rem;
  cursor: pointer;
  background-color: #fff;
}

.sous-famille-content {
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-grow: 1;
  font-size: 0.9em;
}

.articles-list {
  margin: 0;
  background-color: #fafafa;
}

table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.9em;
}

th, td {
  padding: 0.25rem 0.5rem;
  text-align: left;
  border: none;
}

th {
  background-color: #f5f5f5;
  font-weight: 500;
  font-size: 0.85em;
  color: #666;
}

tr:nth-child(even) {
  background-color: #f8f8f8;
}

.quantity-controls {
  display: flex;
  align-items: center;
  gap: 0.25rem;
}

.quantity-shortcuts {
  display: flex;
  gap: 0.15rem;
}

.shortcut-btn {
  padding: 0.15rem 0.25rem;
  font-size: 0.75em;
  background-color: #f5f5f5;
  border: 1px solid #ddd;
  border-radius: 2px;
  cursor: pointer;
  color: #666;
}

.shortcut-btn:hover {
  background-color: #e8e8e8;
}

input[type="number"] {
  width: 50px;
  padding: 0.15rem 0.25rem;
  border: 1px solid #ddd;
  border-radius: 3px;
  font-size: 0.9em;
}

input[type="number"]:focus {
  outline: none;
  border-color: #4CAF50;
  box-shadow: 0 0 0 1px rgba(76, 175, 80, 0.2);
}

.articles-count {
  font-size: 0.8em;
  color: #666;
  margin-left: 0.5rem;
}

.loading, .error {
  padding: 0.5rem;
  text-align: center;
}

.error {
  color: #d32f2f;
  background-color: #ffebee;
  border-radius: 4px;
}

.validation-section {
  position: sticky;
  bottom: 0;
  background-color: white;
  padding: 0.5rem;
  border-top: 1px solid #ddd;
  display: flex;
  align-items: center;
  justify-content: flex-end;
  gap: 1rem;
  box-shadow: 0 -2px 4px rgba(0,0,0,0.1);
}

.connection-status {
  font-size: 0.85em;
  color: #4CAF50;
  display: flex;
  align-items: center;
}

.connection-status.offline {
  color: #f44336;
}

.pending-changes {
  font-size: 0.85em;
  color: #666;
}

.validate-button {
  padding: 0.5rem 1rem;
  background-color: #4CAF50;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 0.9em;
}

.validate-button:disabled {
  background-color: #ccc;
  cursor: not-allowed;
}

.validate-button:not(:disabled):hover {
  background-color: #45a049;
}

h1 {
  margin: 0;
  font-size: 1.1em;
  flex-grow: 1;
}
</style>
