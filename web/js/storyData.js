// C.A.R.E - Cegah Api, Rawat Ekosistem
// Complete Story, Dialogue, Level & Encyclopedia Database from Figma & Unity

const STORY_PROLOGUE = [
  {
    id: 1,
    speaker: "NARATOR",
    text: "Hutan tropis menjadi tempat hidup bagi berbagai jenis tumbuhan dan satwa. Setiap bagian ekosistem memiliki peran dalam menjaga keseimbangannya.",
    portrait: null,
    bg: "assets/figma/bg_story.png",
    cardPos: "bottom"
  },
  {
    id: 2,
    speaker: "NARATOR",
    text: "Namun, sebagian kawasan hutan kini mengalami kerusakan akibat pembukaan lahan yang tidak bertanggung jawab dan kebakaran.",
    portrait: null,
    bg: "assets/figma/bg_story.png",
    cardPos: "bottom"
  },
  {
    id: 3,
    speaker: "NARATOR",
    text: "Api dan asap mengubah keadaan hutan. Tempat mencari makan, berlindung, dan berkembang biak bagi satwa ikut terancam.",
    portrait: null,
    bg: "assets/figma/bg_story.png",
    cardPos: "bottom"
  },
  {
    id: 4,
    speaker: "NARATOR",
    text: "Satwa liar tidak selalu dapat berpindah dengan mudah ketika habitatnya terganggu.",
    portrait: null,
    bg: "assets/figma/bg_story.png",
    cardPos: "bottom"
  },
  {
    id: 5,
    speaker: "NARATOR",
    text: "Sebagian kehilangan tempat berlindung dan sumber makanan.",
    portrait: null,
    bg: "assets/figma/bg_story.png",
    cardPos: "bottom"
  },
  {
    id: 6,
    speaker: "NARATOR",
    text: "Sebagian lainnya terpaksa menjauh dari wilayah tempat mereka hidup.",
    portrait: null,
    bg: "assets/figma/bg_story.png",
    cardPos: "bottom"
  },
  {
    id: 7,
    speaker: "NARATOR",
    text: "Di antara keadaan tersebut, masih ada satwa yang membutuhkan pertolongan.",
    portrait: null,
    bg: "assets/figma/bg_story.png",
    cardPos: "bottom"
  },
  {
    id: 8,
    speaker: "NARATOR",
    text: "Seorang penjelajah tiba di kawasan tersebut setelah mengetahui bahwa hutan mengalami kebakaran.",
    portrait: null,
    bg: "assets/figma/bg_story.png",
    cardPos: "bottom"
  },
  {
    id: 9,
    speaker: "PENJELAJAH",
    text: "Kondisi hutan ini lebih parah dari yang kubayangkan. Aku harus memeriksa keadaan di sekitar.",
    portrait: "assets/figma/penjelajah_portrait.png",
    bg: "assets/figma/bg_story.png",
    cardPos: "bottom"
  },
  {
    id: 10,
    speaker: "NARATOR",
    text: "Seorang penjelajah tiba di kawasan tersebut setelah mengetahui bahwa hutan mengalami kebakaran.",
    portrait: null,
    bg: "assets/figma/bg_story.png",
    cardPos: "bottom"
  },
  {
    id: 11,
    speaker: "NARATOR",
    text: "Di dekat pepohonan yang rusak, penjelajah menemukan seekor satwa yang masih berada di lokasi.",
    portrait: null,
    bg: "assets/figma/bg_story.png",
    cardPos: "bottom"
  },
  {
    id: 12,
    speaker: "PENJELAJAH",
    text: "Tenang. Aku tidak akan menyakitimu. Aku akan mencari cara agar kamu bisa dibawa ke tempat yang lebih aman.",
    portrait: "assets/figma/penjelajah_portrait.png",
    bg: "assets/figma/bg_story.png",
    cardPos: "bottom"
  },
  {
    id: 13,
    speaker: "NARATOR",
    text: "Penjelajah memutuskan untuk membantu satwa yang terdampak sekaligus menelusuri petunjuk di sekitar hutan.",
    portrait: null,
    bg: "assets/figma/bg_story.png",
    cardPos: "center"
  },
  {
    id: 14,
    speaker: "NARATOR",
    text: "Sebelum melanjutkan perjalanan, penjelajah memastikan satwa tersebut mendapat pertolongan yang diperlukan.",
    portrait: null,
    bg: "assets/figma/bg_story.png",
    cardPos: "center"
  },
  {
    id: 15,
    speaker: "UI / SISTEM",
    text: "PERJALANAN DIMULAI!",
    portrait: null,
    bg: "assets/figma/bg_story.png",
    cardPos: "center",
    isFinalIntro: true
  }
];

const LEVELS = [
  {
    id: 1,
    title: "Selamatkan Orangutan!",
    targetAnimal: "orangutan",
    animalName: "Orangutan",
    themeClass: "level-card-orangutan",
    bgGradient: "linear-gradient(180deg, #bf9957 0%, #7a623e 100%)",
    borderColor: "#002c4f",
    unlocked: true,
    bossIntroPose: "assets/characters/doctor_sawit_man_throw.png",
    bossCombatPose: "assets/characters/doctor_sawit_man_throw.png",
    bossDefeatPose: "assets/characters/doctor_sawit_man_spell.png",
    bossHp: 1000,
    targetScore: 1200,
    introDialog: [
      { speaker: "NARATOR", text: "Setelah mengumpulkan petunjuk dari dua lokasi, penjelajah menemukan seseorang yang tampaknya mengetahui lebih banyak tentang perubahan kawasan ini.", portrait: null },
      { speaker: "DOCTOR SAWIT MAN", text: "Kalian sudah mengumpulkan cukup banyak catatan. Seharusnya kalian berhenti sampai di sini.", portrait: "assets/characters/doctor_sawit_man_throw.png" },
      { speaker: "PENJELAJAH", text: "Kami menemukan perubahan pada kawasan hutan dan dampaknya terhadap satwa. Kami ingin tahu siapa yang bertanggung jawab.", portrait: "assets/figma/penjelajah_portrait.png" },
      { speaker: "DOCTOR SAWIT MAN", text: "Kalian hanya melihat sebagian kecil dari persoalan ini. Kalian tidak tahu apa yang sedang kalian hadapi.", portrait: "assets/characters/doctor_sawit_man_throw.png" },
      { speaker: "UI", text: "HADAPI DOCTOR SAWIT MAN. Padukan balok elemen untuk memadamkan api dan menyelamatkan Orangutan!" }
    ],
    defeatDialog: [
      { speaker: "DOCTOR SAWIT MAN", text: "Kalian belum memahami seluruh ceritanya. Ini belum berakhir...", portrait: "assets/characters/doctor_sawit_man_spell.png" },
      { speaker: "PENJELAJAH", text: "Kami akan terus menelusuri petunjuk yang ada. Orangutan ini sekarang aman bersama kami!", portrait: "assets/figma/penjelajah_portrait.png" },
      { speaker: "NARATOR", text: "Doctor Sawit Man mundur. Orangutan berhasil diselamatkan dan Level 2 kini terbuka!", portrait: null }
    ]
  },
  {
    id: 2,
    title: "Selamatkan Gajah Asia!",
    targetAnimal: "gajah_asia",
    animalName: "Gajah Asia",
    themeClass: "level-card-gajah",
    bgGradient: "linear-gradient(180deg, #00a716 0%, #006006 100%)",
    borderColor: "#002c4f",
    unlocked: false,
    bossIntroPose: "assets/characters/doctor_sawit_man_flask_up.png",
    bossCombatPose: "assets/characters/doctor_sawit_man_flask_up.png",
    bossDefeatPose: "assets/characters/doctor_sawit_man.png",
    bossHp: 1500,
    targetScore: 2000,
    introDialog: [
      { speaker: "NARATOR", text: "Catatan dan dokumentasi dari Stage sebelumnya membantu penjelajah menelusuri lokasi berikutnya.", portrait: null },
      { speaker: "DOCTOR SAWIT MAN", text: "Kalian masih mengikuti jejak itu? Aku sudah memperingatkan kalian untuk berhenti!", portrait: "assets/characters/doctor_sawit_man_flask_up.png" },
      { speaker: "PENJELAJAH", text: "Petunjuk dari lokasi berbeda saling berkaitan. Kami tidak akan mengabaikan kerusakan yang menimpa Gajah Asia!", portrait: "assets/figma/penjelajah_portrait.png" },
      { speaker: "DOCTOR SAWIT MAN", text: "Ramuan kimia dan pembukaan lahan ini demi keuntungan! Kalian tidak bisa menghentikanku!", portrait: "assets/characters/doctor_sawit_man_flask_up.png" },
      { speaker: "UI", text: "DOCTOR SAWIT MAN KEMBALI MENGHADANG. Padukan elemen untuk mengalahkannya!" }
    ],
    defeatDialog: [
      { speaker: "DOCTOR SAWIT MAN", text: "Kalian semakin dekat, tetapi masih ada satu bagian yang belum kalian lihat...", portrait: "assets/characters/doctor_sawit_man.png" },
      { speaker: "PENJELAJAH", text: "Gajah Asia telah mendapatkan koridor aman! Kami akan menyelesaikan misi pemulihan ini!", portrait: "assets/figma/penjelajah_portrait.png" },
      { speaker: "NARATOR", text: "Doctor Sawit Man kembali mundur. Level 3 (Pertarungan Terakhir) kini terbuka!", portrait: null }
    ]
  },
  {
    id: 3,
    title: "Kalahkan Dr. Sawit Man!",
    targetAnimal: "harimau",
    animalName: "Harimau Sumatera",
    themeClass: "level-card-sawit",
    bgGradient: "linear-gradient(180deg, #bc0000 0%, #630000 100%)",
    borderColor: "#4f0001",
    unlocked: false,
    bossIntroPose: "assets/characters/doctor_sawit_man_cannon.png",
    bossCombatPose: "assets/characters/doctor_sawit_man_cannon.png",
    bossDefeatPose: "assets/characters/doctor_sawit_man_crossed.png",
    bossHp: 2200,
    targetScore: 3000,
    introDialog: [
      { speaker: "NARATOR", text: "Dari seluruh petunjuk yang dikumpulkan, penjelajah akhirnya menemukan markas utama pembukaan lahan.", portrait: null },
      { speaker: "DOCTOR SAWIT MAN", text: "Kalian berhasil sampai sejauh ini. Dengan meriam sawit ini, aku akan selesaikan semuanya!", portrait: "assets/characters/doctor_sawit_man_cannon.png" },
      { speaker: "PENJELAJAH", text: "Keputusan yang tidak bertanggung jawab merusak masa depan ekosistem kita. Kami tidak akan tinggal diam!", portrait: "assets/figma/penjelajah_portrait.png" },
      { speaker: "DOCTOR SAWIT MAN", text: "Mari kita buktikan tekad siapa yang lebih kuat!", portrait: "assets/characters/doctor_sawit_man_cannon.png" },
      { speaker: "UI", text: "PERTARUNGAN TERAKHIR! Kalahkan Dr. Sawit Man dan lindungi seluruh satwa rimba!" }
    ],
    defeatDialog: [
      { speaker: "DOCTOR SAWIT MAN", text: "Aku mengakui bahwa tindakanku telah menyebabkan kerusakan. Aku harus bertanggung jawab atas keputusan itu...", portrait: "assets/characters/doctor_sawit_man_crossed.png" },
      { speaker: "PENJELAJAH", text: "Mengakui kesalahan adalah langkah awal. Pemulihan hutan membutuhkan tindakan nyata dan pengawasan bersama.", portrait: "assets/figma/penjelajah_portrait.png" },
      { speaker: "NARATOR", text: "Konfrontasi berakhir dengan damai. C.A.R.E berhasil menyelamatkan rimba tropis dan seluruh satwa!", portrait: null }
    ]
  }
];

const ANIMAL_DOSSIERS = {
  orangutan: {
    name: "Orangutan",
    latin: "Pongo pygmaeus",
    image: "assets/animals/orangutan.png",
    desc: "Orangutan (Pongo sp.) adalah kera besar endemik Indonesia yang hidup arboreal atau menghabiskan sebagian besar waktunya di atas pepohonan hutan hujan tropis Kalimantan dan Sumatera, serta kini berstatus sangat terancam punah. Sebagai satwa yang sangat bergantung pada kanopi hutan untuk mencari buah-buahan, bersarang, dan berpindah tempat, kehidupan mereka sangat rentan terhadap bencana kebakaran hutan dan lahan (karhutla). Karhutla secara masif memusnahkan pohon-pohon sumber pakan dan tempat tinggal mereka, menyebabkan kematian langsung akibat terjebak api atau menghirup kabut asap pekat, serta memaksa orangutan turun ke tanah dan memasuki area perkebunan warga yang berujung pada konflik dan perburuan.",
    taxonomy: {
      famili: "Hominidae",
      genus: "Pongo",
      spesies: "Pongo pygmaeus"
    },
    habitat: "Kanopi hutan hujan tropis (arboreal) di Pulau Kalimantan dan Sumatera.",
    makanan: "Omnivora: Buah-buahan, daun muda, madu, dan serangga."
  },
  gajah_asia: {
    name: "Gajah Asia",
    latin: "Elephas maximus",
    image: "assets/animals/gajah_asia.png",
    desc: "Gajah asia (Elephas maximus)—termasuk subspesies endemik gajah sumatera dan gajah kalimantan—adalah mamalia darat terbesar di Asia yang dikenal cerdas, hidup berkelompok, serta membutuhkan wilayah jelajah yang sangat luas dan koridor hutan yang saling terhubung untuk mencari pakan dan air. Karena kebutuhan ruang hidup dan konsumsi tumbuhan yang sangat tinggi setiap harinya, satwa berstatus terancam punah ini sangat terdampak oleh kebakaran hutan dan lahan (karhutla). Karhutla memusnahkan sumber pakan utama dan sumber air bersih mereka sekaligus memutus jalur migrasi alami, sehingga memaksa kawanan gajah keluar dari hutan menuju area pertanian serta permukiman warga dan memicu konflik berkepanjangan dengan manusia.",
    taxonomy: {
      famili: "Elephantidae",
      genus: "Elephas",
      spesies: "Elephas maximus sumatranus"
    },
    habitat: "Hutan tropis, padang rumput, dan semak belukar di Asia Selatan & Tenggara (termasuk Sumatera & Kalimantan).",
    makanan: "Herbivora: Rumput, dedaunan, ranting, kulit kayu, akar, dan buah-buahan."
  },
  harimau: {
    name: "Harimau Sumatera",
    latin: "Panthera tigris sumatrae",
    image: "assets/animals/HarimauSumateraKesakitan.png",
    desc: "Harimau sumatera (Panthera tigris sumatrae) adalah subspesies harimau terkecil di dunia sekaligus satu-satunya yang tersisa di Indonesia, yang kini berstatus kritis terancam punah. Sebagai predator puncak yang membutuhkan hutan luas untuk bertahan hidup, keberadaan satwa endemik ini sangat terancam oleh kebakaran hutan dan lahan (karhutla). Bencana karhutla tidak hanya menghancurkan habitat serta sumber mangsanya secara langsung, tetapi juga memaksa harimau memasuki kawasan permukiman dan perkebunan warga sehingga memicu konflik mematikan dengan manusia yang mempercepat laju kepunahannya.",
    taxonomy: {
      famili: "Felidae",
      genus: "Panthera",
      spesies: "Panthera tigris sumatrae"
    },
    habitat: "Hutan hujan tropis dataran rendah hingga pegunungan di Pulau Sumatera.",
    makanan: "Karnivora: Rusa, babi hutan, kancil, tapir, dan mamalia kecil."
  }
};
