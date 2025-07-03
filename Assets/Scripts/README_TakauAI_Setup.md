# Takau AI & Player Death System Setup Guide

## Overview
Sistem AI Takau dengan death system untuk player yang sudah terintegrasi dengan animasi dan visual feedback.

## Components

### 1. TakauAI.cs
Script utama untuk AI Takau dengan behavior:
- **Wait**: Menunggu dan mendeteksi player
- **Chase**: Mengejar player dengan smooth acceleration
- **Charge**: Seruduk dengan kecepatan tinggi
- **Attack**: Menyerang player jika dalam range dan angle

### 2. PlayerDeathHandler.cs
Script untuk menangani kematian player:
- Death animation trigger
- Disable player movement dan input
- Audio/Visual effects
- Death UI display
- Reset system untuk restart

### 3. TakauAttackHitCollider.cs
Script untuk collider di tangan Takau:
- Mendeteksi hit dengan player saat attack
- Trigger death sequence
- Visual/Audio feedback

## Setup Instructions

### Player Setup
1. Attach `PlayerDeathHandler.cs` ke Player GameObject
2. Setup references:
   - **Player Animator**: Drag Animator component
   - **Death Animation Trigger**: Nama trigger untuk death animation (contoh: "Death")
   - **Death Animation State**: Nama state untuk death animation
   - **Audio Source**: Component untuk death sound
   - **Death Sound Effect**: AudioClip untuk suara mati
   - **Death Particle Effect**: ParticleSystem untuk efek mati
   - **Death Screen UI**: GameObject UI untuk screen mati

### Takau Setup
1. Attach `TakauAI.cs` ke Takau GameObject
2. Setup NavMeshAgent component
3. Setup Animator dengan trigger "Attack"
4. Configure all parameters di inspector:
   - **Current Target**: Drag Player Transform
   - **Movement speeds** (chase, charge)
   - **Vision settings** (detection range, angles)
   - **Attack settings** (range, angle, hit collider)

### Takau Hand Collider Setup
1. Buat child GameObject di tangan Takau (contoh: "AttackHitCollider")
2. Add Collider component (set as Trigger)
3. Attach `TakauAttackHitCollider.cs`
4. Setup reference ke TakauAI script
5. Set Player Tag yang benar

## Key Features

### AI Behavior States
- **WAIT**: Idle state, mencari player dalam detection radius
- **CHASE**: Mengejar player dengan smooth acceleration
- **CHARGE**: Seruduk dengan speed tinggi ke arah player
- **ATTACK**: Menyerang jika player dalam range dan angle

### Vision System
- Detection radius dengan angle vision
- Forward vision bonus untuk attack
- Charge search vision yang expanding
- Visual gizmos untuk debugging

### Attack System
- Range-based attack dengan angle check
- Forward vision bonus untuk extended range
- Hit collider activation saat animation frame tertentu
- Prevent multiple hits per attack

### Death System
- Smooth integration antara AI dan Player
- Death animation dengan duration control
- Disable player controls saat mati
- Audio/Visual feedback
- Reset system untuk restart

## Debug Features

### Visual Debug (Gizmos)
- Detection radius (cyan circle)
- Vision cones untuk different states
- Attack range visualization
- Charge vision areas
- Current state dan target info

### GUI Debug
- Real-time AI state information
- Attack timing dan hit collider status
- Player death state
- Manual test buttons

## Animation Requirements

### Player Animations
- **Death**: Trigger animation untuk player mati
- Set animation duration di PlayerDeathHandler

### Takau Animations
- **Attack**: Trigger animation untuk Takau attack
- Configure hit collider timing sesuai animation frames

## Tips untuk Integration

1. **NavMesh**: Pastikan area game memiliki NavMesh yang proper
2. **Tags**: Set player tag dengan benar ("Player")
3. **Layers**: Configure collision layers jika diperlukan
4. **Animation Events**: Bisa gunakan Animation Events untuk timing yang lebih presisi
5. **Sound Manager**: Integrate dengan sound system game
6. **Game Manager**: Connect dengan checkpoint/restart system

## Troubleshooting

### Player tidak mati saat diserang
- Check PlayerDeathHandler component pada player
- Verify player tag sama dengan setting di TakauAttackHitCollider
- Check hit collider setup di tangan Takau

### AI tidak bergerak
- Check NavMeshAgent component
- Verify NavMesh coverage di scene
- Check current target assignment

### Animation tidak play
- Verify animator controller setup
- Check trigger names sesuai dengan script
- Check animation state names

### Hit collider tidak bekerja
- Verify collider isTrigger = true
- Check TakauAI reference di TakauAttackHitCollider
- Check player tag matching

---

*Script dibuat untuk Unity project "Dunia Sebrang" dengan namespace DS*
