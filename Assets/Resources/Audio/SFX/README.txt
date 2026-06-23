Bu klasoere asagidaki isimlerle ses dosyasi koyun (uzanti onemli degil: .wav / .mp3 / .ogg hepsi calisir,
kod dosya adini uzantisiz olarak Resources.Load ile yukler). AudioManager.Play("isim") cagrildiginda
bu klasorden otomatik yuklenir, Inspector'da hicbir baglama gerekmez.

piece_swap        - Iki tasin yer degistirdigi an calan kisa "kayma/tik" sesi (60-150ms, yumusak).
piece_swap_invalid- Eslesme olmadan tas geri donerken calan kisa "reddedildi/buzz" sesi.
piece_match       - Tas(lar) eslesip temizlendiginde calan parlak "pop/chime" sesi.
ice_crack         - Donmus (buzlu) tas catladiginda/kirildiginda calan cam/buz kirilma sesi.
star_earned       - Skor bir yildiz esigini astiginda calan kisa, parlak "ding" sesi.
level_win         - Seviye kazanildiginda calan kisa zafer fanfari/jingle.
level_lose        - Seviye kaybedildiginde calan dusuk tonlu "womp" sesi.
button_click      - Her UI butonuna basildiginda calan kisa tik sesi.
bonus_moves       - Reklam izleyip bonus hamle kazanildiginda calan pozitif "odul/coin" sesi.
level_unlock      - Reklamla yeni seviye kilidi acildiginda calan kilit/parlama sesi.
battle_hit        - Battle Path'te dugume guc uygulanan her "vurus" aninda calan darbe sesi.
battle_node_clear - Battle Path dugumu yenildiginde calan zafer/patlama sesi.
