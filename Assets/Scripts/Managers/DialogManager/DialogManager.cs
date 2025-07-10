using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using DS.Data.Dialog;

namespace DS
{
    public class DialogManager : MonoBehaviour
    {
        public static DialogManager Instance { get; private set; }
        public DialogUI ui;
        
        private HashSet<string> triggeredDialogs = new();
        private bool isPlayingDialog = false;
        private Queue<DialogRequest> dialogQueue = new();
        
        // Struct untuk menyimpan request dialog
        [System.Serializable]
        public struct DialogRequest
        {
            public DialogData data;
            public int index;
            public DialogTrigger trigger; // Reference ke trigger yang meminta dialog
            
            public DialogRequest(DialogData data, int index, DialogTrigger trigger = null)
            {
                this.data = data;
                this.index = index;
                this.trigger = trigger;
            }
        }
        
        private void Awake()
        {
            if (Instance == null) 
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else 
            {
                Destroy(gameObject);
                return;
            }
            
            ui = FindFirstObjectByType<DialogUI>();
            if (ui == null)
            {
                Debug.LogError("DialogUI not found!");
                return;
            }
            ui.HideDialog();
            
            Debug.Log("DialogManager initialized");
        }
        
        public void RequestDialog(DialogData data, int index, DialogTrigger trigger = null)
        {
            Debug.Log($"RequestDialog called: {data.name} - Index: {index} - IsPlaying: {isPlayingDialog}");
            
            // Cek oneTimePlay hanya jika trigger ada dan sudah dimainkan
            if (data.oneTimePlay && trigger != null && trigger.HasBeenPlayed()) 
            {
                Debug.Log($"Dialog already played from trigger: {data.name}");
                return;
            }
            
            // Cek oneTimePlay untuk dialog yang dipanggil dari kode lain
            if (data.oneTimePlay && trigger == null && triggeredDialogs.Contains($"{data.Id}_{index}")) 
            {
                Debug.Log($"Dialog already played: {data.Id}_{index}");
                return;
            }
            
            if (index < 0 || index >= data.dialogLines.Count) 
            {
                Debug.LogError($"Invalid dialog index: {index} for {data.name}");
                return;
            }
            
            DialogRequest newRequest = new DialogRequest(data, index, trigger);
            
            // Jika sedang tidak ada dialog yang berjalan, langsung play
            if (!isPlayingDialog)
            {
                Debug.Log($"Playing dialog immediately: {data.name}");
                StartCoroutine(PlayDialogCoroutine(newRequest));
            }
            else
            {
                // Jika dialog sedang berjalan, tambahkan ke queue
                Debug.Log($"Adding dialog to queue: {data.name} - Queue size: {dialogQueue.Count}");
                dialogQueue.Enqueue(newRequest);
            }
        }
        
        // Backward compatibility - untuk kode yang masih menggunakan method lama
        public void PlaySpecificLine(DialogData data, int index)
        {
            RequestDialog(data, index, null);
        }
        
        private IEnumerator PlayDialogCoroutine(DialogRequest request)
        {
            Debug.Log($"Starting dialog: {request.data.name} - Duration: {request.data.dialogLines[request.index].duration}");
            isPlayingDialog = true;
            
            // Cek apakah trigger masih aktif sebelum memulai dialog (hanya jika ada trigger)
            if (request.trigger != null && !request.trigger.IsPlayerInTrigger())
            {
                Debug.Log($"Player not in trigger, skipping dialog: {request.data.name}");
                ProcessNextDialog();
                yield break;
            }
            
            DialogLine line = request.data.dialogLines[request.index];
            string speaker = string.IsNullOrEmpty(line.speakerName) ? "" : line.speakerName + ": ";
            
            ui.ShowDialog(speaker + line.text);
            
            if (line.voiceClip)
                AudioSource.PlayClipAtPoint(line.voiceClip, Vector3.zero);
            
            // PENTING: Tandai sebagai sudah dimainkan DI AWAL, bukan di akhir
            if (request.data.oneTimePlay)
            {
                if (request.trigger != null)
                {
                    // Untuk trigger, gunakan method OnDialogPlayed
                    request.trigger.OnDialogPlayed();
                }
                else
                {
                    // Untuk dialog dari kode lain, gunakan triggeredDialogs
                    triggeredDialogs.Add($"{request.data.Id}_{request.index}");
                }
            }
            
            // Tunggu durasi dialog sampai selesai (tidak peduli player keluar atau tidak)
            yield return new WaitForSeconds(line.duration);
            
            ui.HideDialog();
            
            Debug.Log($"Dialog finished: {request.data.name}");
            ProcessNextDialog();
        }
        
        private void ProcessNextDialog()
        {
            Debug.Log($"ProcessNextDialog called - Queue size: {dialogQueue.Count}");
            isPlayingDialog = false;
            
            // Proses dialog berikutnya dalam queue
            while (dialogQueue.Count > 0)
            {
                DialogRequest nextRequest = dialogQueue.Dequeue();
                Debug.Log($"Processing queued dialog: {nextRequest.data.name}");
                
                // Cek apakah trigger masih aktif (hanya jika ada trigger)
                if (nextRequest.trigger == null || nextRequest.trigger.IsPlayerInTrigger())
                {
                    Debug.Log($"Starting queued dialog: {nextRequest.data.name}");
                    StartCoroutine(PlayDialogCoroutine(nextRequest));
                    return;
                }
                else
                {
                    Debug.Log($"Skipping queued dialog (player not in trigger): {nextRequest.data.name}");
                }
                // Jika trigger tidak aktif, skip dialog ini dan coba yang berikutnya
            }
            
            Debug.Log("No more dialogs in queue");
        }
        
        public bool IsDialogPlaying()
        {
            return isPlayingDialog;
        }
        
        public void ClearDialogQueue()
        {
            dialogQueue.Clear();
        }
        
        // Method untuk debugging
        public int GetQueueCount()
        {
            return dialogQueue.Count;
        }
    }
}