using System;
using System.Collections.Generic;
using UnityEngine;

namespace CARE.Story
{
    [Serializable]
    public class DialogueLine
    {
        public string speaker;     // "NARATOR", "PENJELAJAH", "DOCTOR SAWIT MAN", "UI / PETUNJUK"
        [TextArea(2, 5)]
        public string text;
        public string portraitKey; // "penjelajah", "sawit_man", "none"
        public string sfxCue;

        public DialogueLine(string speaker, string text, string portraitKey = "none", string sfxCue = "")
        {
            this.speaker = speaker;
            this.text = text;
            this.portraitKey = portraitKey;
            this.sfxCue = sfxCue;
        }
    }

    [Serializable]
    public class HiddenItemData
    {
        public string itemName;
        [TextArea(2, 4)]
        public string popupNote;
        public string spriteName;

        public HiddenItemData(string itemName, string popupNote, string spriteName)
        {
            this.itemName = itemName;
            this.popupNote = popupNote;
            this.spriteName = spriteName;
        }
    }

    [Serializable]
    public class AnimalEncyclopediaData
    {
        public string animalKey;
        public string commonName;
        public string scientificName;
        [TextArea(2, 4)] public string taxonomy;
        [TextArea(2, 4)] public string habitat;
        [TextArea(2, 4)] public string diet;
        [TextArea(2, 4)] public string openingQuote;
        public bool isUnlocked;

        public AnimalEncyclopediaData(string key, string common, string scientific, string taxonomy, string habitat, string diet, string quote, bool unlocked = false)
        {
            this.animalKey = key;
            this.commonName = common;
            this.scientificName = scientific;
            this.taxonomy = taxonomy;
            this.habitat = habitat;
            this.diet = diet;
            this.openingQuote = quote;
            this.isUnlocked = unlocked;
        }
    }

    public static class CareStoryDatabase
    {
        // ----------------- OPENING STORY SCENES -----------------
        public static List<DialogueLine> Scene01_HutanTerbakar = new List<DialogueLine>()
        {
            new DialogueLine("NARATOR", "Hutan tropis menjadi tempat hidup bagi berbagai jenis tumbuhan dan satwa. Setiap bagian ekosistem memiliki peran dalam menjaga keseimbangannya."),
            new DialogueLine("NARATOR", "Namun, sebagian kawasan hutan kini mengalami kerusakan akibat pembukaan lahan yang tidak bertanggung jawab dan kebakaran."),
            new DialogueLine("NARATOR", "Api dan asap mengubah keadaan hutan. Tempat mencari makan, berlindung, dan berkembang biak bagi satwa ikut terancam.")
        };

        public static List<DialogueLine> Scene02_SatwaTerdampak = new List<DialogueLine>()
        {
            new DialogueLine("NARATOR", "Satwa liar tidak selalu dapat berpindah dengan mudah ketika habitatnya terganggu."),
            new DialogueLine("NARATOR", "Sebagian kehilangan tempat berlindung dan sumber makanan. Sebagian lainnya terpaksa menjauh dari wilayah tempat mereka hidup."),
            new DialogueLine("NARATOR", "Di antara keadaan tersebut, masih ada satwa yang membutuhkan pertolongan.")
        };

        public static List<DialogueLine> Scene03_PenjelajahTiba = new List<DialogueLine>()
        {
            new DialogueLine("NARATOR", "Seorang penjelajah tiba di kawasan tersebut setelah mengetahui bahwa hutan mengalami kebakaran."),
            new DialogueLine("PENJELAJAH", "Kondisi hutan ini lebih parah dari yang kubayangkan. Aku harus memeriksa keadaan di sekitar.", "penjelajah"),
            new DialogueLine("PENJELAJAH", "Pertama, aku perlu memastikan apakah masih ada satwa yang membutuhkan bantuan.", "penjelajah")
        };

        public static List<DialogueLine> Scene04_SatwaTerluka = new List<DialogueLine>()
        {
            new DialogueLine("NARATOR", "Di dekat pepohonan yang rusak, penjelajah menemukan seekor satwa yang masih berada di lokasi."),
            new DialogueLine("PENJELAJAH", "Tenang. Aku tidak akan menyakitimu. Aku akan mencari cara agar kamu bisa dibawa ke tempat yang lebih aman.", "penjelajah"),
            new DialogueLine("NARATOR", "Sebelum melanjutkan perjalanan, penjelajah memastikan satwa tersebut mendapat pertolongan yang diperlukan.")
        };

        public static List<DialogueLine> Scene05_MisiDimulai = new List<DialogueLine>()
        {
            new DialogueLine("PENJELAJAH", "Kamu sudah berada di tempat yang lebih aman. Setelah ini, aku akan mencari tahu apa yang menyebabkan kerusakan di kawasan ini.", "penjelajah"),
            new DialogueLine("NARATOR", "Penjelajah memutuskan untuk membantu satwa yang terdampak sekaligus menelusuri petunjuk di sekitar hutan."),
            new DialogueLine("NARATOR", "Perjalanan pemulihan dimulai dari satu lokasi. Setiap petunjuk akan membantu mengungkap keadaan yang sebenarnya.")
        };

        // ----------------- LOBBY RESTORASI -----------------
        public static List<DialogueLine> LobbyRestorasiIntro = new List<DialogueLine>()
        {
            new DialogueLine("NARATOR", "Setelah meninggalkan lokasi kebakaran, penjelajah tiba di habitat restorasi."),
            new DialogueLine("NARATOR", "Tempat ini akan menjadi rumah yang lebih aman bagi satwa yang berhasil diselamatkan."),
            new DialogueLine("NARATOR", "Beberapa satwa masih belum terbuka. Selesaikan misi pada setiap Stage untuk memperoleh reward dan membuka satwa baru."),
            new DialogueLine("UI / PETUNJUK", "Pilih satwa yang sudah terbuka untuk melihat informasi. Tekan PLAY untuk memulai misi berikutnya.")
        };

        // ----------------- BOSS DIALOGUES (STAGE 1-3) -----------------
        public static List<DialogueLine> BossStage1_Intro = new List<DialogueLine>()
        {
            new DialogueLine("NARATOR", "Setelah mengumpulkan petunjuk dari dua lokasi, penjelajah menemukan seseorang yang tampaknya mengetahui lebih banyak tentang perubahan kawasan ini."),
            new DialogueLine("DOCTOR SAWIT MAN", "Kalian sudah mengumpulkan cukup banyak catatan. Seharusnya kalian berhenti sampai di sini.", "sawit_man"),
            new DialogueLine("PENJELAJAH", "Kami menemukan perubahan pada kawasan hutan dan dampaknya terhadap satwa. Kami ingin tahu siapa yang bertanggung jawab.", "penjelajah"),
            new DialogueLine("DOCTOR SAWIT MAN", "Kalian hanya melihat sebagian kecil dari persoalan ini. Kalian tidak tahu apa yang sedang kalian hadapi.", "sawit_man"),
            new DialogueLine("PENJELAJAH", "Kerusakan habitat tidak bisa dianggap sebagai hal biasa. Kami akan memastikan petunjuk ini diperiksa.", "penjelajah"),
            new DialogueLine("UI / PETUNJUK", "HADAPI DOCTOR SAWIT MAN. Selesaikan tantangan boss untuk melanjutkan misi.")
        };

        public static List<DialogueLine> BossStage1_Defeat = new List<DialogueLine>()
        {
            new DialogueLine("DOCTOR SAWIT MAN", "Kalian belum memahami seluruh ceritanya. Ini belum berakhir.", "sawit_man"),
            new DialogueLine("PENJELAJAH", "Kami akan terus menelusuri petunjuk yang ada.", "penjelajah"),
            new DialogueLine("NARATOR", "Doctor Sawit Man mundur. Petunjuk yang terkumpul membuka jalan untuk melanjutkan investigasi.")
        };

        public static List<DialogueLine> BossStage2_Intro = new List<DialogueLine>()
        {
            new DialogueLine("NARATOR", "Catatan dan dokumentasi dari Stage sebelumnya membantu penjelajah menelusuri lokasi berikutnya."),
            new DialogueLine("DOCTOR SAWIT MAN", "Kalian masih mengikuti jejak itu? Aku sudah memperingatkan kalian untuk berhenti.", "sawit_man"),
            new DialogueLine("PENJELAJAH", "Petunjuk dari lokasi berbeda saling berkaitan. Kami tidak akan mengabaikan kerusakan yang terjadi.", "penjelajah"),
            new DialogueLine("DOCTOR SAWIT MAN", "Satu kawasan yang rusak bukan berarti kalian memahami seluruh persoalannya.", "sawit_man"),
            new DialogueLine("PENJELAJAH", "Justru karena itu kami mengumpulkan informasi dari beberapa tempat. Semua petunjuk harus diperiksa.", "penjelajah"),
            new DialogueLine("UI / PETUNJUK", "DOCTOR SAWIT MAN KEMBALI MENGHADANG. Selesaikan tantangan boss.")
        };

        public static List<DialogueLine> BossStage2_Defeat = new List<DialogueLine>()
        {
            new DialogueLine("DOCTOR SAWIT MAN", "Kalian semakin dekat, tetapi masih ada satu bagian yang belum kalian lihat.", "sawit_man"),
            new DialogueLine("PENJELAJAH", "Kalau begitu, kami akan mencari bagian yang belum ditemukan itu.", "penjelajah"),
            new DialogueLine("NARATOR", "Doctor Sawit Man kembali mundur. Petunjuk yang diperoleh mengarah pada area pemulihan hutan.")
        };

        public static List<DialogueLine> BossStage3_Intro = new List<DialogueLine>()
        {
            new DialogueLine("NARATOR", "Dari seluruh petunjuk yang dikumpulkan, penjelajah akhirnya menemukan hubungan antara keputusan pembukaan lahan dan kerusakan yang terjadi di kawasan hutan."),
            new DialogueLine("DOCTOR SAWIT MAN", "Kalian berhasil sampai sejauh ini. Apa lagi yang ingin kalian buktikan?", "sawit_man"),
            new DialogueLine("PENJELAJAH", "Petunjuk dari beberapa lokasi menunjukkan pola yang sama. Kami tidak akan membiarkan kerusakan ini diabaikan.", "penjelajah"),
            new DialogueLine("DOCTOR SAWIT MAN", "Aku mengira pembukaan lahan akan memberi keuntungan tanpa menimbulkan akibat sebesar ini.", "sawit_man"),
            new DialogueLine("PENJELAJAH", "Keputusan yang tidak bertanggung jawab berdampak pada hutan dan kehidupan satwa. Dampaknya tidak bisa dihapus begitu saja.", "penjelajah"),
            new DialogueLine("DOCTOR SAWIT MAN", "Aku tidak bisa menyangkal catatan dan dokumentasi yang kalian temukan.", "sawit_man"),
            new DialogueLine("UI / PETUNJUK", "PERTARUNGAN TERAKHIR. Selesaikan tantangan untuk mengakhiri konflik.")
        };

        public static List<DialogueLine> BossStage3_Defeat = new List<DialogueLine>()
        {
            new DialogueLine("DOCTOR SAWIT MAN", "Aku mengakui bahwa tindakanku telah menyebabkan kerusakan. Aku harus bertanggung jawab atas keputusan itu.", "sawit_man"),
            new DialogueLine("PENJELAJAH", "Mengakui kesalahan adalah langkah awal. Pemulihan hutan membutuhkan tindakan nyata dan pengawasan.", "penjelajah"),
            new DialogueLine("DOCTOR SAWIT MAN", "Aku bersedia menyerahkan informasi yang kuberikan dan mendukung proses pemulihan sesuai aturan yang berlaku.", "sawit_man"),
            new DialogueLine("NARATOR", "Konfrontasi berakhir. Namun, pekerjaan untuk memulihkan hutan dan melindungi satwa masih harus dilanjutkan.")
        };

        // ----------------- ENDING SCENES -----------------
        public static List<DialogueLine> Ending_Scene = new List<DialogueLine>()
        {
            new DialogueLine("NARATOR", "Setelah rangkaian misi selesai, satwa yang berhasil diselamatkan mendapat tempat yang lebih aman."),
            new DialogueLine("NARATOR", "Pemulihan hutan membutuhkan waktu. Pepohonan, sumber air, dan tempat berlindung perlu dijaga agar ekosistem dapat kembali berfungsi."),
            new DialogueLine("PENJELAJAH", "Kita sudah membantu beberapa satwa. Tetapi menjaga hutan tidak berhenti setelah satu misi selesai.", "penjelajah"),
            new DialogueLine("NARATOR", "Hutan bukan sekadar kumpulan pepohonan. Hutan adalah rumah bagi berbagai kehidupan."),
            new DialogueLine("NARATOR", "Melindungi hutan berarti menjaga habitat, sumber makanan, dan keseimbangan ekosistem."),
            new DialogueLine("PENJELAJAH", "Setiap tindakan untuk menjaga lingkungan memiliki arti. Mulailah dengan peduli dan bertanggung jawab.", "penjelajah"),
            new DialogueLine("UI / PETUNJUK", "C.A.R.E — CEGAH. API. RAWAT. EKOSISTEM. Terima kasih telah membantu menjaga hutan dan satwa.")
        };

        // ----------------- HIDDEN OBJECT ITEMS & POPUP NOTES -----------------
        public static List<HiddenItemData> Stage1_Level1_Items = new List<HiddenItemData>()
        {
            new HiddenItemData("Kotak P3K", "Perlengkapan pertolongan pertama dapat membantu penanganan awal. Satwa yang terluka tetap perlu diperiksa oleh petugas konservasi yang kompeten.", "kotak_p3k"),
            new HiddenItemData("Botol air", "Air bersih penting bagi manusia dan satwa. Ketersediaan sumber air juga menjadi bagian dari habitat yang perlu dijaga.", "botol_air"),
            new HiddenItemData("Kain bersih", "Perlengkapan sederhana dapat membantu proses penanganan awal. Penanganan satwa liar sebaiknya dilakukan oleh petugas terlatih.", "kain_bersih"),
            new HiddenItemData("Peta kawasan", "Peta membantu menunjukkan lokasi terdampak dan jalur menuju area yang lebih aman.", "peta_kawasan"),
            new HiddenItemData("Jejak kaki satwa", "Jejak dapat menjadi petunjuk bahwa satwa pernah melintas di area tersebut. Pengamatan jejak membantu memahami pergerakan satwa.", "jejak_kaki_satwa")
        };

        public static List<HiddenItemData> Stage1_Level2_Items = new List<HiddenItemData>()
        {
            new HiddenItemData("Sarang kosong", "Pepohonan dapat menjadi tempat berlindung dan berkembang biak bagi banyak satwa. Kerusakan pohon dapat menghilangkan tempat tersebut.", "sarang_kosong"),
            new HiddenItemData("Dahan patah", "Struktur pohon menyediakan jalur dan tempat beraktivitas bagi satwa arboreal. Kerusakan vegetasi mengubah kondisi habitat.", "dahan_patah"),
            new HiddenItemData("Daun terbakar", "Kebakaran dapat merusak tumbuhan yang menjadi bagian dari sumber makanan dan perlindungan satwa.", "daun_terbakar"),
            new HiddenItemData("Catatan pengamatan", "Catatan lapangan membantu membandingkan keadaan habitat sebelum dan sesudah gangguan.", "catatan_pengamatan"),
            new HiddenItemData("Foto habitat", "Dokumentasi visual dapat membantu tim konservasi menilai perubahan kondisi suatu lokasi.", "foto_habitat")
        };

        public static List<HiddenItemData> Stage2_Level1_Items = new List<HiddenItemData>()
        {
            new HiddenItemData("Patok penanda", "Penanda kawasan dapat membantu menunjukkan batas atau titik pengamatan. Maknanya perlu dibaca bersama peta dan dokumen pendukung.", "patok_penanda"),
            new HiddenItemData("Peta lahan", "Peta membantu melihat perubahan penggunaan ruang. Informasi peta perlu diperiksa agar tidak disalahartikan.", "peta_lahan"),
            new HiddenItemData("Foto sebelum", "Dokumentasi kondisi awal dapat digunakan sebagai pembanding untuk melihat perubahan kawasan.", "foto_sebelum"),
            new HiddenItemData("Foto sesudah", "Perbandingan foto membantu mengenali perubahan tutupan vegetasi dan kondisi permukaan tanah.", "foto_sesudah"),
            new HiddenItemData("Lembar pengamatan", "Pengamatan terstruktur membantu mencatat kondisi lapangan secara konsisten.", "lembar_pengamatan")
        };

        public static List<HiddenItemData> Stage2_Level2_Items = new List<HiddenItemData>()
        {
            new HiddenItemData("Buku catatan", "Catatan kegiatan dapat membantu menyusun urutan peristiwa. Informasi harus dibandingkan dengan sumber lain.", "buku_catatan"),
            new HiddenItemData("Kartu identitas fiktif", "Identitas pada dokumen menjadi petunjuk untuk mengetahui pihak yang terkait dalam cerita. Periksa informasi melalui prosedur yang benar.", "kartu_identitas_fiktif"),
            new HiddenItemData("Peta bertanda", "Tanda pada peta menunjukkan area yang perlu diperiksa. Tanda tersebut belum cukup untuk menyimpulkan penyebab kerusakan.", "peta_bertanda"),
            new HiddenItemData("Foto kegiatan", "Foto memberikan gambaran aktivitas di lokasi pada waktu tertentu. Konteks dan tanggal dokumentasi tetap penting.", "foto_kegiatan"),
            new HiddenItemData("Salinan laporan", "Laporan dapat membantu menelusuri keputusan dan pengawasan kegiatan. Isi laporan perlu diperiksa secara menyeluruh.", "salinan_laporan")
        };

        public static List<HiddenItemData> Stage3_Level1_Items = new List<HiddenItemData>()
        {
            new HiddenItemData("Petunjuk jalur aman", "Jalur evakuasi perlu dipilih dengan memperhatikan kondisi lingkungan dan keselamatan satwa.", "petunjuk_jalur_aman"),
            new HiddenItemData("Wadah air", "Air bersih merupakan kebutuhan dasar satwa. Habitat yang baik perlu memiliki akses terhadap sumber air yang sesuai.", "wadah_air"),
            new HiddenItemData("Persediaan pakan", "Kebutuhan pakan setiap satwa berbeda. Pakan harus disesuaikan dengan jenis dan kondisi satwa.", "persediaan_pakan"),
            new HiddenItemData("Daftar satwa", "Pencatatan membantu petugas mengetahui satwa yang sudah ditemukan, diperiksa, dan dipindahkan.", "daftar_satwa"),
            new HiddenItemData("Catatan pemeriksaan", "Pemeriksaan oleh petugas membantu menentukan kebutuhan perawatan dan pemulihan satwa.", "catatan_pemeriksaan")
        };

        public static List<HiddenItemData> Stage3_Level2_Items = new List<HiddenItemData>()
        {
            new HiddenItemData("Bibit tanaman", "Pemulihan vegetasi perlu memperhatikan jenis tumbuhan yang sesuai dengan ekosistem setempat.", "bibit_tanaman"),
            new HiddenItemData("Label tanaman", "Label membantu memastikan jenis bibit tercatat dan dapat dipantau selama proses penanaman.", "label_tanaman"),
            new HiddenItemData("Peta pemulihan", "Peta membantu merencanakan area pemulihan dan memantau perubahan kawasan dari waktu ke waktu.", "peta_pemulihan"),
            new HiddenItemData("Catatan relawan", "Kegiatan pemulihan membutuhkan koordinasi dan pencatatan pekerjaan agar hasilnya dapat dievaluasi.", "catatan_relawan"),
            new HiddenItemData("Dokumentasi akhir", "Dokumentasi membantu membandingkan kondisi kawasan dan mencatat perkembangan pemulihan.", "dokumentasi_akhir")
        };

        // ----------------- ENCYCLOPEDIA ANIMALS -----------------
        public static List<AnimalEncyclopediaData> Animals = new List<AnimalEncyclopediaData>()
        {
            new AnimalEncyclopediaData(
                "rusa_timor",
                "Rusa Timor",
                "Rusa timorensis",
                "Kingdom: Animalia\nFilum: Chordata\nKelas: Mammalia\nOrdo: Artiodactyla\nFamili: Cervidae\nGenus: Rusa\nSpesies: Rusa timorensis",
                "Rusa Timor dapat ditemukan pada habitat seperti savana, padang rumput, hutan musim, dan kawasan yang memiliki kombinasi vegetasi terbuka serta tutupan pohon.",
                "Rusa Timor merupakan herbivora. Makanannya terutama berupa rerumputan dan berbagai tumbuhan liar.",
                "Rusa Timor merupakan salah satu jenis rusa asli Indonesia. Habitat dan ketersediaan tumbuhan berperan penting bagi kelangsungan hidupnya.",
                true // Unlocked after Stage 1 (default unlocked for demo/testing)
            ),
            new AnimalEncyclopediaData(
                "orangutan",
                "Orangutan",
                "Pongo sp.",
                "Kingdom: Animalia\nFilum: Chordata\nKelas: Mammalia\nOrdo: Primates\nFamili: Hominidae\nGenus: Pongo\nSpesies: Pongo sp.",
                "Orangutan hidup secara alami di hutan tropis Sumatra dan Kalimantan. Mereka sangat bergantung pada pepohonan untuk mencari makan dan membuat sarang.",
                "Sebagian besar makanan orangutan berasal dari tumbuhan, terutama buah hutan, daun muda, tunas, kulit kayu, dan serangga.",
                "Orangutan menghabiskan banyak waktu di pepohonan. Keberadaan hutan yang terjaga menyediakan tempat hidup dan sumber makanan bagi mereka.",
                false // Unlocked after Stage 2
            ),
            new AnimalEncyclopediaData(
                "gajah_asia",
                "Gajah Asia",
                "Elephas maximus",
                "Kingdom: Animalia\nFilum: Chordata\nKelas: Mammalia\nOrdo: Proboscidea\nFamili: Elephantidae\nGenus: Elephas\nSpesies: Elephas maximus",
                "Gajah Asia menggunakan beragam habitat, termasuk hutan dataran rendah dan kawasan bervegetasi terbuka yang memiliki akses air berlimpah.",
                "Gajah Asia merupakan herbivora besar. Makanannya dapat berupa rumput, dahan pohon, ranting, kulit kayu, akar, dan buah.",
                "Gajah membutuhkan ruang jelajah, air, dan tumbuhan sebagai sumber makanan. Perubahan habitat dapat memengaruhi akses mereka terhadap kebutuhan tersebut.",
                false // Unlocked after Stage 3
            )
        };
    }
}

