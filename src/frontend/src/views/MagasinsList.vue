<template>
  <div class="magasins-list">
    <div class="header">
      <button class="back-button" @click="goBack">←</button>
      <h1>Magasins - {{ agence }}</h1>
    </div>
    <div class="list" v-if="!loading">
      <div 
        v-for="magasin in magasins" 
        :key="magasin.code"
        class="list-item"
        @click="goToInventaire(magasin)"
      >
        {{ magasin.nom }}
        <div class="magasin-code">{{ magasin.code }}</div>
      </div>
    </div>
    <div v-else class="loading">
      Chargement...
    </div>
    <div v-if="error" class="error">
      {{ error }}
    </div>
  </div>
</template>

<script>
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { API_URL } from '../config'

export default {
  name: 'MagasinsList',
  setup() {
    const magasins = ref([])
    const route = useRoute()
    const router = useRouter()
    const loading = ref(false)
    const error = ref(null)
    const agence = ref(route.params.agence)

    const fetchMagasins = async () => {
      loading.value = true
      error.value = null
      try {
        const response = await fetch(`${API_URL}/api/Agences/${agence.value}/magasins`)
        if (!response.ok) throw new Error('Erreur lors de la récupération des magasins')
        magasins.value = await response.json()
      } catch (e) {
        error.value = e.message
        console.error('Erreur:', e)
      } finally {
        loading.value = false
      }
    }

    const goBack = () => {
      router.back()
    }

    const goToInventaire = (magasin) => {
      console.log('Magasin sélectionné:', magasin);
      
      const params = {
        agence: route.params.agence,
        magasin: magasin.code,
        agenceName: route.params.agenceName || route.params.agence,
        magasinName: magasin.nom
      };
      
      console.log('Navigation vers inventaire avec params:', params);
      
      router.push({
        name: 'feuille-inventaire',
        params,
        replace: false
      });
    }

    onMounted(fetchMagasins)

    return {
      magasins,
      loading,
      error,
      agence,
      goBack,
      goToInventaire
    }
  }
}
</script>

<style scoped>
.magasins-list {
  padding: 1rem;
  max-width: 600px;
  margin: 0 auto;
}

.header {
  display: flex;
  align-items: center;
  margin-bottom: 2rem;
}

.back-button {
  background: none;
  border: none;
  font-size: 1.5rem;
  cursor: pointer;
  padding: 0.5rem;
  margin-right: 1rem;
}

h1 {
  margin: 0;
  color: #2c3e50;
  font-size: 1.5rem;
}

.list {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.list-item {
  background: white;
  padding: 1rem;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);
  cursor: pointer;
}

.magasin-code {
  font-size: 0.8rem;
  color: #666;
  margin-top: 0.5rem;
}

.loading {
  text-align: center;
  padding: 2rem;
  color: #666;
}

.error {
  color: #dc3545;
  text-align: center;
  padding: 1rem;
  margin-top: 1rem;
}
</style>
