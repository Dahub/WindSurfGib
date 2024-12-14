<template>
  <v-app>
    <v-app-bar color="primary">
      <v-app-bar-title>Administration WindSurf</v-app-bar-title>
    </v-app-bar>

    <v-main>
      <v-container>
        <v-row>
          <v-col cols="12">
            <v-card>
              <v-card-title>Gestion du fichier CSV</v-card-title>
              <v-card-text>
                <v-row>
                  <v-col cols="12" md="4">
                    <v-btn
                      color="primary"
                      block
                      :loading="downloading"
                      @click="downloadCsv"
                    >
                      Télécharger le fichier CSV
                    </v-btn>
                  </v-col>

                  <v-col cols="12" md="4">
                    <v-file-input
                      v-model="fileToUpload"
                      label="Sélectionner un fichier CSV"
                      accept=".csv"
                      :loading="uploading"
                      @change="uploadCsv"
                    ></v-file-input>
                  </v-col>

                  <v-col cols="12" md="4">
                    <v-btn
                      color="error"
                      block
                      :loading="deleting"
                      @click="confirmDelete"
                    >
                      Supprimer le fichier CSV
                    </v-btn>
                  </v-col>
                </v-row>
              </v-card-text>
            </v-card>
          </v-col>
        </v-row>

        <!-- Dialog de confirmation -->
        <v-dialog v-model="showDeleteDialog" max-width="500px">
          <v-card>
            <v-card-title>Confirmation</v-card-title>
            <v-card-text>
              Êtes-vous sûr de vouloir supprimer le fichier CSV ?
            </v-card-text>
            <v-card-actions>
              <v-spacer></v-spacer>
              <v-btn color="primary" variant="text" @click="showDeleteDialog = false">
                Annuler
              </v-btn>
              <v-btn color="error" variant="text" @click="deleteCsv">
                Supprimer
              </v-btn>
            </v-card-actions>
          </v-card>
        </v-dialog>

        <!-- Snackbar pour les messages -->
        <v-snackbar v-model="showSnackbar" :color="snackbarColor">
          {{ snackbarMessage }}
          <template v-slot:actions>
            <v-btn
              color="white"
              variant="text"
              @click="showSnackbar = false"
            >
              Fermer
            </v-btn>
          </template>
        </v-snackbar>
      </v-container>
    </v-main>
  </v-app>
</template>

<script lang="ts">
import { defineComponent, ref } from 'vue'
import axios from 'axios'

export default defineComponent({
  name: 'App',

  setup() {
    const fileToUpload = ref<File | null>(null)
    const downloading = ref(false)
    const uploading = ref(false)
    const deleting = ref(false)
    const showDeleteDialog = ref(false)
    const showSnackbar = ref(false)
    const snackbarMessage = ref('')
    const snackbarColor = ref('success')

    const showMessage = (message: string, color: string = 'success') => {
      snackbarMessage.value = message
      snackbarColor.value = color
      showSnackbar.value = true
    }

    const downloadCsv = async () => {
      try {
        downloading.value = true
        const response = await axios.get('/api/admin/csv', {
          responseType: 'blob'
        })
        
        const url = window.URL.createObjectURL(new Blob([response.data]))
        const link = document.createElement('a')
        link.href = url
        link.setAttribute('download', 'inventory.csv')
        document.body.appendChild(link)
        link.click()
        link.remove()
        showMessage('Fichier téléchargé avec succès')
      } catch (error) {
        showMessage('Erreur lors du téléchargement du fichier', 'error')
      } finally {
        downloading.value = false
      }
    }

    const uploadCsv = async () => {
      if (!fileToUpload.value) return

      const formData = new FormData()
      formData.append('file', fileToUpload.value)

      try {
        uploading.value = true
        await axios.post('/api/admin/csv', formData, {
          headers: {
            'Content-Type': 'multipart/form-data'
          }
        })
        showMessage('Fichier uploadé avec succès')
      } catch (error) {
        showMessage('Erreur lors de l\'upload du fichier', 'error')
      } finally {
        uploading.value = false
        fileToUpload.value = null
      }
    }

    const confirmDelete = () => {
      showDeleteDialog.value = true
    }

    const deleteCsv = async () => {
      try {
        deleting.value = true
        await axios.delete('/api/admin/csv')
        showMessage('Fichier supprimé avec succès')
      } catch (error) {
        showMessage('Erreur lors de la suppression du fichier', 'error')
      } finally {
        deleting.value = false
        showDeleteDialog.value = false
      }
    }

    return {
      fileToUpload,
      downloading,
      uploading,
      deleting,
      showDeleteDialog,
      showSnackbar,
      snackbarMessage,
      snackbarColor,
      downloadCsv,
      uploadCsv,
      confirmDelete,
      deleteCsv
    }
  }
})
</script>

<style>
.v-btn {
  text-transform: none;
}
</style>
