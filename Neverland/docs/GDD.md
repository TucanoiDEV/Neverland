# A TERRA DO NUNCA — Documento de Game Design

> **Transcrição fiel** de `C:\Neverland\GDD_A_Terra_do_Nunca.docx` (v1.0, julho de 2026), que segue sendo a fonte oficial.
> Este arquivo é a base do projeto. Toda decisão de código, arte e som deve responder a ele.
> Não convertido: as 4 figuras do original (Fig. 4.1 blockout do térreo · Fig. 5.1 loop central · Fig. 6.1 FSM da perseguidora · Fig. 8.1 curva de tensão) — só as legendas sobreviveram.

| Campo | Valor |
|---|---|
| Título | A Terra do Nunca (codinome: **NEVERLAND**) |
| Versão do doc | 1.0 — rascunho completo para pré-produção |
| Data | Julho de 2026 |
| Autor / Studio | Tucano (TucanoiDEV) · TEREJACKS |
| Engine | Unity 2022.3 LTS (3D, URP) |
| Gênero | Survival horror · Stealth · Escape room |
| Plataformas | PC (Steam / itch.io); consoles em avaliação |
| Classificação | 18+ (gore explícito, temas sensíveis, terror) |
| Status | Pré-produção |

> "Todas as crianças crescem. Menos uma." — a premissa clássica de Peter Pan, aqui relida como ameaça: e se nunca crescer não fosse um presente, mas uma prisão?

**DOCUMENTO CONFIDENCIAL — USO INTERNO**

---

## 01 · Visão Geral

Este capítulo define o que o jogo é em uma página: conceito, pilares, referências e o que o torna único. Toda decisão dos capítulos seguintes deve responder a esta seção.

### 1.1 High Concept

Um survival horror em primeira pessoa, com estética de PS1/VHS, em que um menino chamado **Wendy** foge da realidade violenta de sua casa e acorda na **Terra do Nunca** — um paraíso infantil que, à noite, revela ser uma armadilha. Sem armas, sem combate: apenas itens, esconderijos, enigmas e a coragem de uma criança que só quer voltar para casa.

A Terra do Nunca subverte o conto de Peter Pan: o lugar onde as crianças nunca crescem é, aqui, o lugar de onde as crianças nunca saem. O jogador vive esse horror pela perspectiva baixa e vulnerável de uma criança, com a linguagem visual suja e granulada dos jogos da Puppet Combo e uma fuga linear, curta e sem trégua — o formato dos próprios jogos da Puppet Combo. Os enigmas seguem a lógica de casa-prisão popularizada por Granny.

### 1.2 Ficha rápida

| Aspecto | Definição |
|---|---|
| Gênero | Survival horror · Stealth · Escape room em primeira pessoa |
| Perspectiva | Primeira pessoa (altura de criança, ~1,20 m), com cutscenes em câmera fixa estilo PS1 |
| Combate | Inexistente. O jogador nunca ataca; apenas foge, se esconde e usa itens |
| Duração-alvo | ~40 min por run (formato Puppet Combo) · rejogabilidade por finais, conquistas, speedrun e spawns semialeatórios |
| Estrutura | Linear e contínua: prólogo + 1 dia de falsa paz + uma única noite de fuga + clímax e epílogo |
| Tom | Terror psicológico com fachada de conto de fadas; melancólico, opressor, com gore explícito no padrão Puppet Combo |
| Público-alvo | Fãs de horror indie (Puppet Combo, Chilla's Art, Granny, Dark Deception), streamers, 18+ |
| Modelo | Jogo premium de baixo custo (US$ 4,99–7,99), lançamento em early access opcional |

### 1.3 Pilares de design

Quatro pilares guiam todas as decisões. **Se uma feature não sustenta pelo menos um deles, ela não entra.**

1. **VULNERABILIDADE ABSOLUTA** — Wendy é uma criança. Não há armas, golpes ou contra-ataques. Todo poder do jogador vem de conhecimento (rotas, padrões, enigmas) e itens. O medo nasce da impotência física.
2. **O PARAÍSO É A PRISÃO** — A Terra do Nunca é linda de dia e monstruosa à noite. A direção de arte, o som e o level design vendem a falsa segurança para depois retirá-la. O contraste é a principal arma de horror do jogo.
3. **TENSÃO POR RUÍDO E ROTINA** — Como em Granny, o perigo é regido por som e rotina: cada tábua que range, gaveta que bate ou vidro que quebra chama a perseguidora. O jogador aprende a casa, e a casa aprende o jogador.
4. **NOSTALGIA CORROMPIDA (PS1/VHS)** — Low poly, texturas de baixa resolução, dithering, CRT e fitas caseiras. A estética Puppet Combo não é filtro: é linguagem narrativa — a memória de infância degradada como uma fita gasta.

### 1.4 Referências principais

| Referência | O que aproveitamos | O que NÃO aproveitamos |
|---|---|---|
| Puppet Combo (catálogo) | Estética PS1/VHS, câmeras de cutscene, sound design cru, ritmo de perseguição, brevidade (~40 min), gore explícito | Controles-tanque como padrão (aqui, apenas opcionais) |
| Granny | Lógica dos enigmas de casa-prisão, progressão por itens, audição do inimigo | Estrutura de múltiplas noites e humor involuntário; aqui a fuga é linear e o tom é sério |
| Outlast (Red Barrels) | Impotência total (sem combate), esconde-esconde sob pressão, escuridão real navegada com uma fonte de luz de recurso limitado | Câmera com bateria e found footage como dispositivo (aqui, VHS é estética, não gadget) |
| Silent Hill 2 | Estrutura deste GDD, horror como metáfora psicológica, trilha que conta história | Combate e sistema de armas |
| Resident Evil 2/3 (Mr. X / Nemesis) | Perseguidor persistente que invade o espaço seguro do jogador | Escala AAA e set pieces de ação |
| Peter Pan – La Obscura Verdad (curta) | A releitura sombria do mito; a cena do penhasco que inspira o clímax | — |
| Little Nightmares / Among the Sleep | Horror pela perspectiva e escala de uma criança | Plataforma 2.5D / física de puzzle |

### 1.5 O que torna o jogo único (USPs)

- **Releitura de domínio público com identidade própria:** Peter Pan como culto infantil de horror — reconhecível no primeiro segundo do trailer.
- **Protagonista criança e desarmado em 3D primeira pessoa:** nicho pouco explorado no formato Puppet Combo.
- **Duas faces do mesmo mapa:** o mesmo parque encantado explorado de dia (tutorial diegético) vira o labirinto da noite.
- **Perseguidora transformista:** Sininho alterna entre fada guia (dia) e monstro caçador (noite) — a mesma personagem, dois sistemas.
- **Final autoral e impactante,** fiel ao roteiro original do projeto, com forte potencial de repercussão em streams.

### 1.6 Experiência-alvo do jogador

Em uma sessão ideal (~40 minutos, de ponta a ponta, sem cortes), o jogador deve sentir, nesta ordem: **acolhimento** (prólogo), **encantamento desconfiado** (o dia na ilha), **pânico** (revelação da Sininho), **tensão crescente** (a fuga pela casa), **euforia** (o escape) e, por fim, um **soco emocional silencioso** (o penhasco e o epílogo). O jogo cabe inteiro em uma única sentada — o formato clássico da Puppet Combo: curto o bastante para uma live, denso o bastante para ficar na cabeça por dias.

---

## 02 · História

A narrativa abaixo é o **roteiro canônico** do jogo, organizado em atos. Os aspectos de gameplay citados aqui são apenas âncoras; o detalhamento sistêmico está nos capítulos 5 a 8.

### 2.1 Premissa

Wendy, um menino de cerca de 9 anos, vive em um lar marcado pela violência doméstica. Para sobreviver emocionalmente, desenvolveu um ritual: **fechar os olhos e fugir para dentro das próprias memórias felizes**. Numa dessas fugas, ele abre os olhos em outro lugar — a Terra do Nunca. O que parece um sonho de proteção se revela, numa única noite, um cativeiro do qual nenhuma criança jamais saiu.

### 2.2 Temas

- **Fuga da realidade** — O escapismo como mecanismo de defesa infantil — e o preço de nunca mais voltar.
- **Infância aprisionada** — A promessa de "nunca crescer" lida como sequestro: um paraíso que não deixa ninguém partir.
- **Falsos protetores** — Figuras que acolhem para controlar. Peter Pan e Sininho espelham dinâmicas de abuso: carinho público, ameaça privada.
- **Sacrifício e liberdade** — Escapar exige perder algo de si — literalizado na cena da mão. A liberdade de Wendy tem um custo permanente.

> **Nota de sensibilidade:** o jogo trata violência doméstica de forma implícita (sons atrás de uma porta, nunca em cena). O objetivo é empatia, não choque. Uma cartela com recursos de ajuda (ex.: Disque 100 / 180 no Brasil) é exibida nos créditos.

### 2.3 Sinopse estruturada em atos

**› PRÓLOGO — O quarto real**
Interior noite. O quarto de Wendy, iluminado apenas pela luz que vaza da porta entreaberta. A mãe, sentada à beira da cama, recita o Pai-Nosso. Ao terminar, deseja boa noite e fecha a porta com um baque seco. Segundos depois, através da parede: vozes alteradas, móveis arrastados, sons de agressão — o pai contra a mãe. Wendy aperta os olhos. O jogador vê, em flashes granulados de VHS, memórias felizes do menino: um aniversário, um parque, a mãe rindo. A tela afunda no escuro. Uma voz sussurra. Wendy abre os olhos.

**› ATO I — O DIA (a chegada e a falsa paz)**
Wendy desperta em um campo dourado sob um céu impossível. Tudo é perfeito — perfeito demais. Uma fada se aproxima: "Você finalmente acordou!". Ela se apresenta como Sininho e explica que estão na Terra do Nunca, "um lugar mágico e especial". Sininho o guia por um caminho margeado de brinquedos gigantes, como um parque de diversões sem fim, cruzando com incontáveis fadas e muitas, muitas crianças.

Eles chegam a uma roda de crianças sentadas. No centro, um menino em pé: Peter Pan. Ele se oferece para mostrar o lugar. Durante o passeio (tutorial diegético de exploração), Wendy nota estranhezas — crianças que sorriem sempre igual, brinquedos que rangem sozinhos, nenhum adulto, nenhuma saída — mas cala-se, achando que sonha. Ao final, Peter Pan entrega a Wendy uma lista de brinquedos: o menino brinca em cada um (mini-interações que ensinam os controles). Ao entardecer, Peter Pan ordena, com doçura que não admite recusa: "Agora, vá dormir."

**› ATO II — A NOITE · 1. A revelação**
Madrugada. Wendy acorda no quarto das crianças da grande casa e entende: continua na Terra do Nunca. Isso não é um sonho. Em desespero, tenta fugir impulsivamente — e cruza com Sininho no corredor. Ao perceber a fuga, a fada se contorce e cresce numa transformação grotesca: asas rasgadas, corpo alongado, o sininho do pescoço agora um badalo surdo. Nasce a perseguidora do jogo — implacável como Nemesis ou Mr. X. Wendy corre. A casa se fecha. Começa a fuga — contínua, sem cortes, até a porta da saída.

**› ATO II — A NOITE · 2. A fuga pela casa**
Ainda na mesma noite, Wendy vasculha a casa de Peter Pan em silêncio: encontra itens, resolve enigmas em sequência, descobre segredos das crianças que vieram antes dele e destrava, peça por peça, a tranca tripla da saída — sempre caçado pela Sininho monstruosa. Cada cômodo aberto aprofunda o horror: bilhetes de crianças antigas, jaulas no porão, uma lista de nomes riscados. (Gameplay detalhada nos caps. 5–8: enigmas na lógica de Granny, estrutura linear no formato Puppet Combo.)

**› ATO II — A NOITE · 3. O penhasco (clímax)**
Com a porta finalmente aberta, Wendy escapa da casa e corre pela Terra do Nunca noturna — o parque encantado agora um cemitério de brinquedos. Peter Pan o avista de longe. Não corre: caminha, calmo, inevitável. Encurralado na beira do penhasco sobre o mar, sem saída, Wendy se lança. Um segundo antes da água, a mão de Peter Pan agarra seu braço — enquadramento inspirado no curta *Peter Pan – La Obscura Verdad*. Pendurado sobre o abismo, Wendy saca o **facão** — encontrado no açougue do porão — e corta a própria mão para se libertar. Ele cai nas águas escuras. *(Sem o facão, não há salvação: ver Finais, cap. 8.3.)*

**› EPÍLOGO — Os crocodilos**
Na beira do penhasco, Sininho alcança Peter Pan e pergunta o que deve ser feito. Peter Pan a consola, sereno, olhando o mar: não há motivo para preocupação — os crocodilos resolverão o problema. Corte para preto. O som de água. Um tique-taque distante. Créditos.

### 2.4 O final e suas leituras

O desfecho é deliberadamente ambíguo e assim deve permanecer em todo o material do jogo:

- **Leitura literal:** Wendy escapou da ilha, mutilado, e seu destino no mar fica em aberto — os crocodilos como ameaça final.
- **Leitura psicológica:** a Terra do Nunca é a mente de Wendy; cortar a mão é o custo de abandonar a fantasia e voltar a uma realidade que também o machuca. Os crocodilos são a realidade que o espera.

> **Diretriz: o jogo NUNCA confirma nenhuma leitura. Nada de cena pós-créditos explicativa.**

### 2.5 Diálogo e Lucidez (voz do protagonista)

Nos momentos de conversa, o jogador escolhe as falas de Wendy — e cada escolha revela o quanto o menino ainda acredita na fantasia ou já a atravessou com o olhar. Aceitar a Terra do Nunca ("Ebaa, não posso esperar para me divertir!") mantém o disfarce; resistir a ela ("Mas eu não quero brincar…", "Sinto saudade da minha mãe") acumula **Lucidez**. A ilha percebe. Quanto mais lúcido Wendy soa, mais Peter Pan e Sininho deixam a máscara escorregar — a doçura vira vigilância, a guia vira sombra. É a forma narrativa de dar voz ao protagonista sem tirar dele a impotência: ele pode enxergar a verdade, mas enxergar não o liberta. O sistema mecânico está no cap. 5.9; o roteiro de falas, no Apêndice C.

### 2.6 Documentos encontráveis (narrativa ambiental)

A história profunda da ilha é contada por **18 colecionáveis opcionais**, no padrão de survival horror clássico:

| Tipo | Exemplos | Função narrativa |
|---|---|---|
| Bilhetes de crianças | "O João tentou sair pelo mar. A Sininho ficou triste." | Provar que Wendy não é o primeiro — e que ninguém saiu |
| Desenhos infantis | Crayon: fadas com dentes; um menino sem a mão | Prenunciar a transformação da Sininho e o final |
| A Lista | Nomes de crianças; a maioria riscada; "Wendy" recém-escrito | Transformar o quadro de avisos em ameaça |
| Páginas do "diário" de Peter Pan | Regras da ilha escritas em caligrafia infantil perfeita demais | Caracterizar Peter Pan como carcereiro que se crê protetor |
| Fitas VHS (bônus) | Trechos distorcidos das "boas-vindas" de outras crianças | Recompensa de exploração; conecta a estética VHS à ficção |

---

## 03 · Personagens

Fichas completas do elenco. Modelos 3D seguem a direção de arte do cap. 10: low poly (500–1.500 tris), texturas 128–256 px, animações rígidas propositalmente "de marionete".

### 3.1 Wendy — o protagonista

| Campo | Descrição |
|---|---|
| Idade / papel | 9 anos · protagonista jogável |
| Aparência | Franzino; pijama azul desbotado de dinossauros; descalço a partir da noite; olheiras |
| Personalidade | Quieto, observador, imaginativo; coragem que nasce do medo, não da força |
| Ferida central | Testemunha da violência do pai contra a mãe; culpa por "fugir" para dentro da própria cabeça |
| Arco | Da fuga passiva (fechar os olhos) à fuga ativa (abrir a porta): aprender que escapar de verdade custa caro |
| No sistema | Sem ataques; corre pouco (barra de fôlego curta), se esconde em espaços pequenos que adultos não alcançam, rasteja por passagens exclusivas de criança |
| Voz | Sem falas audíveis no gameplay (respiração, choramingos); fala apenas em cutscenes, pouco |
| Nota de design | O nome "Wendy" num menino é intencional e **nunca é explicado** — primeira pista de que algo está "fora do lugar" no mundo do jogo |

### 3.2 Sininho — a fada / a perseguidora

**› Forma fada (dia)**

| Campo | Descrição |
|---|---|
| Aparência | Pequena (25 cm), luz âmbar pulsante, vestido de folhas, sino minúsculo ao pescoço; bonita "como uma lembrança" |
| Comportamento | Guia, elogia, vigia. Sempre por perto demais. Seu sino tilinta quando Wendy se aproxima de áreas proibidas |
| Função | Tutorial diegético e falsa aliada; o jogador deve gostar dela antes de temê-la |

**› Forma monstro (noite) — a caçadora**

| Campo | Descrição |
|---|---|
| Gatilho da transformação | Testemunhar qualquer tentativa de fuga (canonicamente: a fuga impulsiva de Wendy na noite) |
| Aparência | 2,4 m; corpo de fada esticado como cera derretida; asas rasgadas que arrastam no chão; o sino, agora grave, **badala a cada passo** — é o "tema sonoro" da ameaça (função do coração de Mr. X / Nemesis) |
| Comportamento | Perseguidora persistente da casa: patrulha, investiga ruídos, persegue, captura (FSM completa no cap. 6) |
| **Regra de ouro** | **Nunca some do mapa durante a noite; só "perde" o jogador, nunca desiste** |
| Fraquezas | Não rasteja (passagens baixas são seguras); luz forte a atordoa por 2 s (lanterna de corda, cap. 7); armadilhas sonoras a deslocam |
| Tragédia | Colecionáveis sugerem que ela já foi criança da ilha — a primeira. **Nunca confirmado** |

### 3.3 Peter Pan — o antagonista

**› Forma menino (o anfitrião)**

| Campo | Descrição |
|---|---|
| Aparência | Menino eterno de ~12 anos; roupas de folhas impecáveis; sorriso constante que nunca chega aos olhos; flutua a 3 cm do chão (detalhe sutil, sem alarde) |
| Personalidade | Anfitrião perfeito, dono absoluto. Não grita, não corre, não ameaça: ordena com doçura. **A calma dele é o terror** |
| Filosofia | Crê genuinamente que salva as crianças do mundo dos adultos. Sair da ilha é, para ele, uma traição — e um perigo do qual as protege |
| No sistema | **NÃO usa a IA de perseguição.** Aparece apenas em momentos roteirizados (dia, ordem de dormir, clímax). Sua presença congela o jogo: quando ele está em cena, correr é impossível (peso narrativo por design) |
| Referência de atuação | Vilões serenos: o tom do curta *La Obscura Verdad*; nunca "vilão de desenho" |

**› Forma dark (o que há sob a fantasia)**

| Campo | Descrição |
|---|---|
| Natureza | Não é uma transformação de combate como a da Sininho: é a verdade de Peter Pan vazando pela máscara. Ele nunca "vira" o monstro na frente do jogador de propósito — o jogo apenas deixa a fachada escorregar até que, no penhasco, ela cai de vez |
| Aparência | A silhueta de menino se alonga e enegrece; a sombra projetada tem braços longos demais e dedos afilados; sob o sorriso, fileiras de dentes pequenos; os olhos — sempre vazios — passam a refletir a luz como os de um animal. Mantém a elegância: é belo e errado ao mesmo tempo, nunca grotesco à toa |
| Vislumbres (ao longo do jogo) | Aparições subliminares que escalam com a Lucidez (cap. 5.9): um reflexo em vidro que não corresponde à pose dele; a sombra na parede que se move meio segundo atrasada; um frame de rosto errado quando a câmera passa por ele; a voz doce ganhando um segundo tom grave por baixo. Sempre negável como "susto meu" — até deixar de ser |
| Revelação (clímax) | No penhasco, ao agarrar o braço de Wendy, a forma dark se assume por inteiro: a mão que segura é a garra alongada, o sorriso abre além do humano, a fala sobre os crocodilos sai na voz dupla. É o único momento em que o jogo confirma o que os vislumbres sugeriram |
| No sistema | **Encenação pura** — efeito visual e sonoro, sem IA nem perseguição. Não altera controles, ameaça nem dificuldade; Peter Pan continua o antagonista roteirizado que caminha (cap. 6.2). O medo vem do reconhecimento, não de uma nova mecânica |
| **Diretriz de contenção** | Os vislumbres devem ser raros e curtos — no máximo um punhado na run inteira. Superexpor a forma dark a transforma em "inimigo comum" e mata o efeito. **Regra: se o jogador tem certeza do que viu, foi vislumbre demais** |
| Clímax | A caminhada lenta; a mão (garra) que agarra Wendy no penhasco; a fala final sobre os crocodilos — ou, se Wendy chega sem o facão, o desfecho em que ele o devora (8.3) |

### 3.4 Elenco de apoio

| Personagem | Ficha resumida |
|---|---|
| **A Mãe** | Presente apenas no prólogo (voz e silhueta) e nas memórias-VHS. A oração do Pai-Nosso que ela recita retorna, distorcida, na trilha da noite. Emocionalmente, é o "lar" pelo qual Wendy luta para voltar — mesmo sendo um lar quebrado |
| **O Pai** | Nunca visto. Existe somente como som atrás da porta no prólogo. **Diretriz: jamais mostrar; o horror real do jogo permanece fora de quadro** |
| **As Crianças** | Dezenas, todas de pijama, todas "felizes". De dia: figurantes que repetem frases prontas ("Aqui é o melhor lugar do mundo!"). À noite: dormem em fileiras perfeitas; algumas sussurram dicas se Wendy se aproximar sem ruído — **o único "sistema de dicas" do jogo é diegético** |
| **As Fadas menores** | Vaga-lumes de rostos borrados que decoram o cenário durante o dia. À noite, somem — dormentes dentro das lanternas da vila. A ausência delas é parte do horror: no escuro da casa, a única luz que resta é a da lanterna de Wendy |
| **Os Crocodilos** | Jamais aparecem por inteiro: olhos na água, um dorso, o tique-taque clássico vindo do mar (relógio engolido — herança do conto). São a fronteira do mundo: **entrar na água = game over instantâneo em qualquer ponto do jogo**. No epílogo, tornam-se a última imagem-ameaça |

### 3.5 Mapa de relações

- **Peter Pan → Sininho:** dono e instrumento. Ele nunca suja as mãos; ela existe para que ele não precise.
- **Sininho → Wendy:** afeto possessivo que vira caça. "Se você fugir, você me machuca."
- **Peter Pan → Wendy:** colecionador e peça nova. Wendy só importa enquanto obedece.
- **Crianças → Wendy:** espelhos do que ele se tornará se ficar — e a razão de o jogador querer sair.

---

## 04 · Mundo e Cenários

O jogo se passa em três macro-ambientes: o Quarto Real (prólogo), a Terra do Nunca aberta (dia e clímax) e a Casa de Peter Pan (a noite — o coração do jogo).

### 4.1 O Quarto Real (prólogo)

| Aspecto | Definição |
|---|---|
| Escopo | Um único cômodo jogável por ~4 minutos; interação limitada (olhar, encolher-se na cama) |
| Paleta / luz | Azul noturno; única fonte de luz é a fresta da porta — que se apaga quando ela se fecha |
| Áudio | Oração da mãe em primeiro plano; depois, a briga abafada com mixagem "atrás da parede" (nunca inteligível) |
| Função | Estabelecer empatia e o ritual de **fechar os olhos** (mecânica-cerimônia que transita para a ilha) |

### 4.2 A Terra do Nunca aberta

Uma ilha-parque sem bordas visíveis além do mar. De dia, saturada e dourada; à noite, dessaturada, enevoada e hostil. **O layout é o mesmo nas duas fases** — o jogador deve reconhecer, apavorado, os lugares onde brincou.

**› Zonas do mapa aberto**

| Zona | De dia (Ato I) | De madrugada (clímax) |
|---|---|---|
| Campo do Despertar | Gramado dourado onde Wendy acorda; flores altas | Grama morta; ponto de partida da fuga final |
| Roda das Crianças | Círculo de crianças sentadas; apresentação de Peter Pan | Círculo vazio com marcas no chão |
| Parque dos Brinquedos | Carrossel, roda-gigante, xícaras, escorregadores gigantes (a "lista de brinquedos") | Labirinto de silhuetas; brinquedos rangem sozinhos; rota de perseguição do clímax |
| Vila das Fadas | Lanternas vivas; enxames dourados | Lanternas apagadas; enxames dormentes — silêncio e breu |
| Trilha do Penhasco | Bloqueada por Sininho ("lá não, bobinho!") | Cenário do clímax: beira de rocha sobre o mar negro |
| O Mar | Horizonte bonito e intocável | Fronteira letal; tique-taque submerso; epílogo |

**› Regras do mundo aberto**

- **Sem mapa na tela:** orientação por marcos visuais (roda-gigante = norte). **Coerência espacial é sagrada.**
- De dia o jogador não pode se afastar das rotas: Sininho o "chama de volta" (limite diegético, sem paredes invisíveis).
- O clímax reaproveita **100% do mapa do dia** — nenhum corredor novo, apenas nova luz, nova trilha e a caminhada de Peter Pan.

### 4.3 A Casa de Peter Pan (mapa principal — a noite)

Uma casa grande demais, de madeira escura e proporções levemente erradas (portas altas demais, corredores estreitos demais). **Três andares + porão.** É o tabuleiro da fuga: o jogador a conhece de dia, iluminada e cheia de crianças, e a redescobre no escuro, cômodo a cômodo, numa única noite.

*Fig. 4.1 — Layout de referência do térreo (blockout). Andares superiores e porão listados na legenda; plantas próprias serão produzidas no greybox.*

**› Princípios de level design da casa**

- **Loop duplo** — Todo cômodo importante tem 2 entradas; nenhuma perseguição termina em beco sem saída injusto (exceto armários — esconder tem risco).
- **Verticalidade** — Escada central conecta os 3 andares e o porão; **ruídos atravessam andares** (pisar forte no 1º andar alerta a Sininho no térreo).
- **Passagens de criança** — Dutos, vãos sob móveis e um alçapão na cozinha: rotas exclusivas de Wendy, seguras porém lentas e claustrofóbicas (a Sininho espreita pelas frestas).
- **Progressão por fechaduras** — A casa inteira está tecnicamente acessível desde o início da noite, mas trancada em camadas (cadeados, tábuas, fusível queimado, corrente congelada) — cada enigma resolvido abre ~25% a mais da casa.
- **A casa "viva"** — A cada enigma resolvido, pequenos objetos mudam de lugar e um novo detalhe perturbador aparece (mais um nome riscado na Lista; uma cama a menos). **Nunca alterar rotas — só a atmosfera.**

### 4.4 O Quarto de Peter Pan

Trancado do início ao fim; só se abre com a **chave do quarto** (enigma 7.3-D). Por dentro, é o oposto da casa "perfeita": completamente empoeirado e bagunçado — brinquedos quebrados, cortinas fechadas há décadas, poeira em suspensão — e, sobre a cama, **um corpo em decomposição que nenhum personagem jamais comenta**. O cômodo guarda o **selo de cera**, o **fusível**, o **diário** e a **chave de ferro do açougue**. *Diretriz de som: nenhuma música ali; apenas a respiração de Wendy.*

### 4.5 O Porão — o açougue

O porão está "liberado" desde o início da noite — mas só é alcançável com itens: o alçapão da cozinha exige o **pé de cabra**, e a porta interna do açougue exige a **chave de ferro** (encontrada no quarto de Peter Pan). São duas alas:

- **Ala do GERADOR (obrigatória)** — alimenta a tranca elétrica da saída e esconde a rota de escape pelo esgoto.
- **AÇOUGUE (opcional)** — um salão de abate inspirado no quarto do Pyramid Head em Silent Hill 2: ganchos, bancadas manchadas, jaulas e corpos de crianças mortas e torturadas. É o único lugar onde a verdade da ilha é explícita e onde o gore do jogo atinge o teto. Pendurado entre os ganchos está o **FACÃO** — o item que decide o final (cap. 8.3).

O design deve tratar o porão como "a nota mais grave" do jogo.

---

## 05 · Sistema de Jogo

Regras e mecânicas. **Princípio absoluto: Wendy nunca ataca.** Todo o sistema existe para transformar silêncio, observação e itens em progresso.

### 5.1 Câmera e perspectiva

- Gameplay em primeira pessoa, altura de câmera **1,20 m** (escala de criança: maçanetas ao nível dos olhos, adultos e o monstro sempre "para cima").
- Head-bob sutil ligado ao fôlego; intensifica ao correr, treme ao se esconder com a Sininho próxima.
- Cutscenes e momentos de Peter Pan em **câmeras fixas** de ângulos baixos/altos, estilo PS1 (Puppet Combo) — reforça impotência.
- FOV padrão **70°** (ajustável 60–90° nas opções).

### 5.2 Controles (PC — referência)

| Ação | Teclado/Mouse | Observações |
|---|---|---|
| Mover | WASD | Velocidade caminhada: **2,2 m/s** |
| Correr | Shift (segurar) | **3,6 m/s**; consome fôlego; MUITO barulhento |
| Agachar / rastejar | Ctrl (alternar) | **1,1 m/s**; quase silencioso; acessa passagens de criança |
| Interagir / pegar | E ou clique esquerdo | Contextual: abrir, puxar, girar, empurrar |
| **Interação lenta** | Segurar E | Abrir portas/gavetas devagar = sem ruído (**núcleo do stealth**) |
| **Fechar os olhos** | Espaço (segurar) | Mecânica-assinatura: reduz o terror visual, aguça a audição (ver 5.6) |
| Inventário rápido | Roda do mouse / 1–4 | **Sem pausa:** o jogo continua enquanto o jogador escolhe |
| Usar item | Clique direito | Item equipado na mão |
| Espiar | Q / E (inclinar) | Espiar esquinas e frestas de armário |
| Soltar item | G | Itens podem ser largados para criar ruído/distração |

### 5.3 Movimentação, fôlego e ruído

- **Fôlego** — Barra **invisível** de ~6 s de corrida; recupera em 8 s parado, 12 s andando. Sem fôlego: Wendy arfa ALTO (raio de ruído dobrado) — **correr é sempre uma dívida**.
- **Modelo de ruído** — Cada ação emite um valor de ruído com raio em metros (tabela 6.1). Superfícies modulam: tapete ×0,5 · madeira ×1,0 · **tábua solta ×2,0** (marcadas por rangido de aviso ao pisar devagar).
- **Peso de itens** — Itens grandes (pé de cabra, martelo) ocupam as duas mãos: Wendy não rasteja carregando-os — decisões de logística constantes.

### 5.4 Esconder-se

Esconderijos são a defesa central: debaixo de camas, dentro de armários, atrás de cortinas e no baú de brinquedos. Regras:

- Entrar num esconderijo **à vista da Sininho não funciona** — ela viu, ela puxa Wendy para fora (morte).
- Dentro do esconderijo, **minigame de respiração**: manter o cursor numa zona que encolhe conforme o monstro se aproxima; falhar = ruído.
- Esconder-se **3 vezes no mesmo esconderijo do mesmo cômodo ensina a Sininho**: ela passa a conferir aquele cômodo na patrulha (anti-exploit e horror crescente).
- Esconderijos de rastejo (dutos, vão do alçapão) são **invioláveis**, porém escuros e sem visão — ela pode esperar do lado de fora.

### 5.5 Inventário e itens

- **4 slots + 1 slot de "item de chave"** (itens de enigma não ocupam slots comuns).
- Interface diegética: Wendy abre a mochila escolar na frente do corpo; **o mundo NÃO pausa**.
- Itens são a única "arma": distrair (caixinha de música), atrasar (cadeira na maçaneta), iluminar (lanterna de corda), abrir (chaves/ferramentas). **Nenhum item causa dano.**

### 5.6 Mecânica-assinatura: Fechar os Olhos

No prólogo, fechar os olhos é como Wendy **foge**. No jogo, é como ele **enfrenta**.

- Segurar Espaço: a tela escurece até restar um fio de luz; sons ganham nitidez e direção (mix binaural); **o badalo da Sininho vira um "sonar" preciso**.
- **Custo: cegueira real.** Andar de olhos fechados é possível — cego, porém guiado pelo som: útil para "ler" a posição exata da Sininho através das paredes antes de cruzar uma área aberta.
- Narrativamente, a mecânica inverte o ritual do trauma: o que era fuga vira **coragem sensorial**.

### 5.7 Captura, morte e save

| Evento | Consequência |
|---|---|
| Capturado pela Sininho | Morte em cutscene curta de câmera fixa, com gore explícito no padrão Puppet Combo; corte para estática de VHS e retorno ao último checkpoint |
| Tocar a água do mar | Game over instantâneo (tique-taque + escuridão) em qualquer ponto do jogo |
| Visto por Peter Pan fora de hora (dia) | Sem game over: ele apenas "corrige" Wendy e o leva de volta — mais assustador que morrer |
| Save | Checkpoints automáticos em marcos de enigma e transições de ato. **Sem save manual** (tensão estilo Puppet Combo); a run de ~40 min compensa |

### 5.8 O loop central

`explorar → item → enigma → ruído calculado → esconder → avançar`

*Fig. 5.1 — Loop central da fuga.*

O jogador alterna entre fases de silêncio (coleta de informação) e picos de risco (ações barulhentas que destravam progresso). O design de cada trecho garante **ao menos 1 ação inevitavelmente ruidosa** — não existe run 100% furtiva; existe ruído escolhido na hora certa.

### 5.9 Sistema de diálogo e Lucidez

Nos momentos roteirizados de conversa (sobretudo no Dia, com Sininho e Peter Pan), o jogador escolhe entre falas em uma caixa de diálogo. Cada escolha pertence a uma de duas naturezas: falas de **ENTREGA**, em que Wendy aceita a fantasia ("Ebaa, não posso esperar para me divertir!"), e falas de **LUCIDEZ**, em que ele resiste a ela ("Mas eu não quero brincar…", "Sinto saudade da minha mãe"). É um sistema de sabor e de caracterização — não há resposta "certa" nem game over por diálogo — mas ele alimenta um contador oculto que muda como a ilha trata o menino.

**› O contador de Lucidez (L)**

- Lucidez é um valor invisível que começa em 0. Cada fala de Lucidez soma **+1**; falas de entrega não subtraem — **a Lucidez nunca cai**, espelhando que perceber a verdade é irreversível.
- O contador **NÃO aparece na tela**: o jogador sente a Lucidez pelas reações do mundo, nunca por um número ou barra (coerente com a filosofia de HUD, cap. 9).
- Certos ramos de diálogo só se abrem em limiares — ex.: com **L ≥ 2**, Wendy pode retrucar Peter Pan com "Sinto saudade da minha mãe" (roteiro no Apêndice C).
- **Regra de ouro: Lucidez altera comportamento narrativo e reações de NPC — NUNCA os parâmetros de dificuldade da caçada** (raios de ruído, visão, velocidade). A dificuldade só sobe por morte (cap. 6). Assim, ser lúcido nunca é "punido" com um jogo mecanicamente mais difícil.

**› Efeito sobre os NPCs**

| Faixa de L | Peter Pan | Sininho |
|---|---|---|
| **L 0 (Sonhador)** | Anfitrião doce e paternal; elogia Wendy; trata-o como convidado especial | Guia carinhosa e tagarela; fica por perto elogiando |
| **L 1–2 (Desperto)** | A doçura ganha um fundo de correção: pausas, sorriso que segura tempo demais, frases possessivas ("você é NOSSO agora") | Menos elogios, mais vigilância; o sino tilinta antes de ela aparecer, como aviso |
| **L 3+ (Lúcido)** | Frieza sob o verniz: encara Wendy diretamente, comenta que ele "anda pensando demais"; a inspeção do fim do Dia é mais longa e ameaçadora | Aparições silenciosas às suas costas durante o Dia; a forma monstro é prenunciada em vislumbres (reflexos, sombras) antes da transformação da noite |

Os efeitos são de **encenação** (falas, animações, câmera, som, spawns de eventos roteirizados), não de balanceamento. **A noite de fuga é idêntica em L 0 ou L 5 — muda a moldura, não as regras.**

*Detalhe de recompensa:* em L alto, alguns colecionáveis extras de narrativa aparecem (um desenho a mais, um bilhete que só um menino "desperto" notaria). São opcionais e nunca obrigatórios para progredir.

**› Diretrizes de escrita e implementação**

- Toda escolha deve caber em 1–2 linhas, ser lida em voz alta em < 4 s e soar como uma criança de 9 anos — nunca exposição adulta.
- Falas de entrega e de lucidez devem ser **igualmente atraentes**: o jogador escolhe por identificação, não por "qual dá o bônus". O sistema recompensa coerência de personagem, não otimização.
- Técnico: máquina de diálogo simples baseada em nós (grafo), com uma variável inteira `lucidez` persistida no save; condições de ramo por limiar. Nada de árvore gigante. Ver cap. 12 para a implementação em Unity.

---

## 06 · Inimigos e IA

A ameaça é uma só e é perfeita nisso: **a Sininho monstruosa**, perseguidora persistente da casa. Peter Pan e os crocodilos orbitam esse núcleo.

### 6.1 Sininho (forma monstro) — visão geral de IA

*Fig. 6.1 — Máquina de estados da perseguidora, com sentidos e dificuldade dinâmica.*

**› Estados em detalhe**

| Estado | Comportamento |
|---|---|
| **PATRULHA** | Caminha por waypoints com ordem semialeatória, ponderada pelo "mapa de calor" (áreas onde o jogador fez ruído recentemente recebem **+40%** de visitas). Abre 1 porta aleatória por ciclo. O badalo do sino anuncia distância e andar |
| **INVESTIGAÇÃO** | Desloca-se ao ponto exato do último ruído; vasculha o cômodo por **8–14 s**; **35%** de chance de abrir o esconderijo mais próximo do ruído. Ruídos encadeados estendem a busca |
| **PERSEGUIÇÃO** | Linha de visão confirmada por **0,4 s** → grito (stinger) e caça a **1,15×** a velocidade de corrida do jogador. Quebra portas trancadas comuns em 3 s (portas de enigma são invioláveis). **Colisão = MORTE** (5.7) |
| **RETORNO** | Perdeu contato por **6 s**: memoriza a "área quente", vasculha 2 cômodos vizinhos e retoma patrulha enviesada para lá |
| **CAPTURA** | Cutscene de morte (gore, 5.7) e recarga do último checkpoint. Ao recarregar, a Sininho reinicia no andar **OPOSTO** ao do jogador (justiça de ritmo) |

**› Tabela de sentidos e ruído**

| Ação do jogador | Raio | Notas |
|---|---|---|
| Rastejar / interação lenta | 3 m | Praticamente seguro fora do mesmo cômodo |
| Andar | 9 m | Atravessa 1 parede fina; tapetes reduzem à metade |
| Correr | 16 m | Atravessa andares pela escada central |
| Abrir porta/gaveta rápido | 10 m | Interação lenta zera o ruído |
| Tábua solta | 18 m | Marcada por rangido de aviso; memorizável |
| Quebrar vidro / derrubar objeto | 22 m | Sempre gera INVESTIGAÇÃO imediata |
| Caixinha de música (item) | 20 m | Ruído "lançável": redireciona a patrulha por 25 s |

**› Regras de justiça (anti-frustração)**

- A Sininho **NUNCA** spawna no cômodo do jogador nem "teleporta"; todo deslocamento é audível pelo badalo.
- Cone de visão honesto: **110°, 12 m** (6 m no escuro); frestas de armário exigem que ela esteja a <1,5 m olhando diretamente.
- Após recarregar um checkpoint, **20 s de "período frio"** sem perseguição — o jogador respira e replaneja.
- **Dificuldade dinâmica sobe apenas com mortes** (nunca com o tempo): jogadores bons enfrentam a caçadora base.
- A **Lucidez (cap. 5.9) NÃO toca esta IA**: em L alto, a forma-fada de Sininho aparece antes e mais silenciosa no Dia (encenação), mas a caçadora noturna é sempre a mesma. Perceber a verdade muda o clima, não as regras.

### 6.2 Peter Pan (ameaça roteirizada)

- **Sem FSM:** aparece somente em eventos de script (ordem de dormir, inspeções de dia, clímax). **Nunca persegue durante a fuga.**
- **Presença = regra:** quando Peter Pan está em cena, o input de corrida é desabilitado e a câmera pesa (aim drag sutil). O corpo do jogador "obedece" antes do jogador.
- **Reage à Lucidez** (cap. 5.9): em L baixo é o anfitrião doce; em L alto encara Wendy, faz comentários possessivos e a inspeção do fim do Dia se alonga. São mudanças de encenação (falas, câmera, animação) — jamais de parâmetros de perseguição.
- No clímax, sua caminhada usa a **velocidade exata de caminhada do jogador +0,1 m/s**: impossível ganhar distância andando — a fuga exige gastar fôlego perfeitamente.
- **Forma dark (encenação — cap. 3.3):** o sistema dispara vislumbres roteirizados (reflexos, sombra atrasada, frame de rosto errado, segunda voz grave) em gatilhos fixos e com frequência ponderada pela Lucidez. Subsistema puramente audiovisual — não spawna colisor, não persegue, não muda dificuldade. No penhasco, um gatilho final revela a forma por inteiro. **Orçamento rígido:** poucos vislumbres por run, cada um curto.

### 6.3 Os Crocodilos (fronteira viva)

- **Sem modelo de IA:** são um limite de mundo com direção de som (tique-taque cresce perto da água) e scripts do epílogo.
- **Função de design:** tornam o mar — a "saída óbvia" de uma ilha — a única coisa mais assustadora que a casa.

---

## 07 · Itens e Enigmas

Sem armas, os itens são o vocabulário completo do jogador. Três famílias: **Progresso** (abrem a casa), **Ferramentas** (manipulam a caçadora) e **Chave** (destravam a saída final).

### 7.1 Itens de progresso

| Item | Uso | Local (padrão)* |
|---|---|---|
| Chave enferrujada | Abre a despensa (início da fuga) | Gaveta da sala de estar |
| Pé de cabra | Remove tábuas de portas/janelas; alavanca o alçapão | Despensa |
| Martelo de brinquedo | Quebra cofres de porcelana e o cadeado gasto do sótão (**RUIDOSO: 22 m**) | Baú do salão de brinquedos |
| Fusível | Religa a energia do porão (habilita o minigame do gerador) | Quarto de Peter Pan (4.4) |
| Chave do quarto | Abre o quarto de Peter Pan (enigma 7.3-D) | Costurada na almofada do trono do salão |
| Chave de ferro do açougue | Abre a ala do açougue no porão — onde está o facão | Quarto de Peter Pan (4.4) |
| Manivela | Abre a claraboia do sótão (rota alternativa de fuga entre andares) | Jaula do porão |
| Chave do relógio | Dá corda no relógio de pêndulo (enigma 7.3-C) | Atrás do quadro da Lista |
| Óleo de lamparina | Silencia dobradiças de 3 portas à escolha do jogador (estratégia de rota) | Cozinha, prateleira alta (exige empurrar cadeira = ruído) |

\* **Posições semialeatórias:** cada item possui **3 pontos de spawn possíveis**, sorteados por run — rejogabilidade no padrão Granny.

### 7.2 Ferramentas (contra a caçadora)

| Item | Efeito | Limites |
|---|---|---|
| Caixinha de música | Dá corda e larga no chão: toca por 25 s atraindo a Sininho ao ponto | 2 usos por run (corda gasta) |
| Lanterna de corda | Facho forte atordoa a Sininho por 2 s a curta distância | Corda dura 8 s; recarregar faz ruído de 6 m; **NÃO funciona em Peter Pan** |
| Cadeira | Trava porta comum por 3 s contra a perseguição (ela quebra) | Arrastar cadeiras fora da perseguição = ruído de 10 m |
| Sino de brinquedo | Imita o badalo da Sininho: crianças adormecidas sussurram dicas achando que é ela | Frágil: quebra após 3 usos |
| **FACÃO (do açougue)** | **O item que decide o final:** com ele, Wendy corta a própria mão no penhasco (8.3). Pendurado entre os ganchos do açougue — opcional e perdível | Sem uso em gameplay comum: **o jogo NUNCA permite usá-lo como arma** contra a Sininho ou Peter Pan |

### 7.3 Enigmas principais (cadeia linear)

**› A · A Tranca de Boas-Vindas**
Objetivo-tutorial da fuga: pegar a chave enferrujada, abrir a despensa, obter o pé de cabra e remover as tábuas da porta do salão de brinquedos. Ensina interação lenta, ruído e o primeiro encontro completo com a Sininho.

**› B · As Marionetes**
No salão, 5 marionetes de crianças. Bilhetes espalhados descrevem "a ordem em que chegamos à ilha". Pendurar as marionetes na ordem correta abre o compartimento com a chave do 1º andar. Errar faz TODAS baterem palmas ao mesmo tempo (**ruído 22 m**) — enigma com custo, não com bloqueio.

**› C · O Relógio Sem Horas**
O relógio de pêndulo da sala não tem ponteiros pintados: as horas estão nos desenhos infantis colecionáveis (um sol entre duas árvores = 14h, etc.). Dar corda e posicionar o relógio na "hora de dormir" oficial da ilha destrava a porta da biblioteca. Integra colecionáveis à progressão sem torná-los obrigatórios (as dicas também existem, mais difíceis, no ambiente).

**› D · A Chave do Quarto**
A porta do quarto de Peter Pan não tem fechadura visível. O diário, na biblioteca, entrega a pista: *"a casa guarda a chave de quem manda"*. A chave está **costurada na almofada do trono**, no salão de brinquedos — rasgá-la é inevitavelmente ruidoso (**16 m**), forçando o jogador a preparar rota e distração antes. Dentro do quarto (4.4): o corpo na cama, o selo de cera, o fusível e a chave de ferro do açougue. **Nenhum inimigo entra ali** — o horror é todo de cenário.

**› E · O Gerador e a Tranca Tripla**
Abrir o porão (alçapão da cozinha + pé de cabra), instalar o fusível e religar o gerador em um minigame de sequência de disjuntores (cada erro = estouro de **22 m** de ruído), e reunir as 3 peças da tranca da saída: **chave-mestra** (sótão), **engrenagem** (relógio, removível após o enigma C) e **selo de cera** (quarto de Peter Pan). Ao lado do gerador, a porta do açougue (chave de ferro) esconde o FACÃO — opcional agora, decisivo depois. Porta da saída aberta → transição para o clímax.

### 7.4 A fuga final (sequência jogável do clímax)

- Perseguição aberta pelo parque noturno: **sem inventário, sem interações** — apenas correr, gerenciar fôlego e escolher rotas entre os brinquedos.
- Peter Pan caminha em interceptação constante (6.2); a Sininho caça atrás; brinquedos acendem sozinhos, iluminando o caminho errado de propósito.
- No penhasco, cutscene interativa: o salto, a mão que agarra. Se Wendy carrega o facão, um único prompt, seco, sem ícone bonito: **SEGURAR [E] PARA CORTAR** — o jogador executa o custo do final com as próprias mãos. Se não carrega, **nenhum prompt aparece** (8.3).

---

## 08 · Estrutura e Fluxo

A espinha do jogo: ordem dos capítulos, objetivos, curva de tensão e condições de final.

### 8.1 Fluxo geral

| Bloco | Conteúdo | Objetivo do jogador | Duração |
|---|---|---|---|
| Prólogo | Quarto real; oração; a briga; fechar os olhos | Nenhum (cerimônia jogável) | 4–5 min |
| O Dia | Chegada, Sininho, Peter Pan, passeio + lista de brinquedos; anoitecer e ordem de dormir | Cumprir a lista (tutorial diegético) | 8–10 min |
| A Noite · Revelação | Despertar no meio da noite; tentativa de fuga; transformação da Sininho | Sobreviver ao primeiro encontro | 2–3 min |
| A Noite · Fuga (casa) | Enigmas A→E em cadeia; a casa se abre; a Lista ganha o nome de Wendy; quarto de Peter Pan, porão, açougue e facão | Montar a tranca tripla e abrir a saída | 16–20 min |
| A Noite · Clímax | Fuga pelo parque; penhasco; a mão; o corte (ou o desfecho sem facão) | Chegar ao mar | 5–6 min |
| Epílogo | Sininho e Peter Pan; crocodilos; créditos + cartela de apoio | — | 2–3 min |

### 8.2 Curva de tensão

*Fig. 8.1 — Curva de tensão do arco completo (~40 min): um único vale diurno, depois escalada contínua até o penhasco.*

O dia único **não é recheio**: é o vale que devolve contraste ao horror. A partir da transformação, a tensão só cresce — com micro-vales controlados (esconderijos, trechos de silêncio) que impedem a dessensibilização — até o pico do penhasco e o silêncio absoluto do epílogo.

### 8.3 Finais

Há **exatamente dois finais**, decididos por um único fator: Wendy carrega ou não o **FACÃO** do açougue ao chegar ao penhasco. Nenhum aviso, nenhum medidor — apenas a consequência de ter (ou não) descido até o pior lugar da ilha.

| Final | Condição e conteúdo |
|---|---|
| **FINAL BOM — "As Águas" (canônico)** | Chegar ao penhasco **COM** o facão. O salto, a mão de Peter Pan, o prompt de corte, a queda nas águas escuras; epílogo dos crocodilos exatamente como no roteiro do cap. 2 |
| **FINAL RUIM — "Devorado"** | Chegar ao penhasco **SEM** o facão. Não há instrumento, não há prompt, não há salvação: a câmera segura o plano enquanto Peter Pan puxa Wendy lentamente para cima, sereno como sempre — e o devora. Gore no teto do padrão Puppet Combo. Corte para preto; créditos sem música |

> **Nota de design:** o final ruim não é punição por habilidade, e sim **por curiosidade não exercida** — quem evita o açougue evita a verdade, e a ilha cobra. Isso transforma o gore do porão em decisão narrativa, não em choque gratuito.

### 8.4 Rejogabilidade

- Spawns de itens semialeatórios (3 pontos por item) e 2 variações de rota de patrulha por run.
- Timer de speedrun opcional após o primeiro zeramento — o formato de ~40 min convida a runs otimizadas (cultura Puppet Combo).
- **Modo Pesadelo (New Game+):** sem badalo no sino da Sininho (caça silenciosa) e checkpoints apenas no início de cada ato.
- **Conquistas de estilo:** "Nenhum armário" (zerar sem esconderijos), "Mão firme" (chegar ao penhasco com o facão sem morrer nenhuma vez), "Silêncio absoluto" (nunca entrar em PERSEGUIÇÃO).

---

## 09 · Interface

**Filosofia: o mínimo de tela possível.** Toda informação que puder viver no mundo (diegética) vive no mundo.

### 9.1 HUD durante o gameplay

| Elemento | Regra |
|---|---|
| Retículo | Ponto de 2 px que só aparece sobre objetos interativos; vira ícone de mão ao alcance |
| Fôlego | **SEM barra:** comunicado por respiração (áudio), head-bob e vinheta pulsante |
| Vida | **Inexistente** — não há dano gradual; só captura/água |
| Itens | O item selecionado fica VISÍVEL durante todo o gameplay, "levitando" diante da câmera no padrão Puppet Combo (10.3). Caso-limite: a lanterna, que ilumina o ambiente — sem ela equipada, a noite é breu real, impossível de navegar |
| Ameaça | **100% por som (badalo) e luz.** Nunca radar, nunca indicador |
| Objetivo | Caderninho de Wendy (tecla TAB): anotações desenhadas a lápis, atualizadas por evento — o "quest log" é um objeto da ficção |
| Legendas | Opcionais, com indicadores direcionais de som para acessibilidade ([sino à esquerda]) |

### 9.2 Menus

- **Menu principal:** a TV de tubo do quarto real exibindo estática; opções como fita VHS sendo rebobinada; o "NEW GAME" aparece como **PLAY ▶**.
- **Menu de pausa:** apenas no dia; **a noite NÃO pausa** — decisão de tensão, com aviso claro nas opções. Polaroid do cenário atual com opções manuscritas.
- **Tela de game over da água:** apenas o tique-taque em tela preta, 5 segundos, sem texto.

### 9.3 Caixa de diálogo (sistema de Lucidez)

- Aparece só em momentos de conversa roteirizada (cap. 5.9). Visual diegético: balão de fala desenhado a lápis, como se saído do caderninho de Wendy; fonte manuscrita, sem molduras de RPG.
- 2 a 3 opções por vez, **sem rótulos de "bom/mau", sem ícones de karma, sem prévia de consequência**. O jogador escolhe por identificação com o personagem, não por otimização.
- O contador de Lucidez é **INVISÍVEL**: nunca há número, barra ou "ding". O feedback é 100% diegético — a mudança de tom dos NPCs é a única confirmação de que a escolha importou.
- Falas de Lucidez podem receber um leve tremor/estática de VHS ao serem destacadas — um sussurro visual de que "essa é a fala verdadeira" — sem nunca explicitar o sistema.

### 9.4 Opções e acessibilidade

| Categoria | Opções |
|---|---|
| Vídeo | Intensidade do filtro CRT/dithering (0–100%), FOV, brilho com imagem de calibração |
| Áudio | Volumes independentes; modo "áudio de fone" (binaural) recomendado ao iniciar |
| Acessibilidade | Legendas direcionais; modo daltônico para códigos de cor de luz e itens; reduzir head-bob; segurar→alternar em todos os inputs; **modo "História"** (Sininho 20% mais lenta e período frio dobrado) claramente rotulado |
| Conteúdo | Aviso de temas sensíveis no primeiro boot; opção de pular o prólogo em NG+ |

---

## 10 · Direção de Arte

A estética é PS1/VHS à la Puppet Combo — **não como filtro nostálgico, mas como linguagem**: a infância de Wendy é uma fita gravada por cima.

### 10.1 Diretrizes gerais

- **Low poly honesto** — Personagens 500–1.500 tris; adereços 50–300 tris. **Silhueta acima de detalhe:** tudo precisa ser legível como sombra.
- **Texturas** — 64–256 px, pintadas à mão, filtragem point (pixelada); paleta limitada por cena.
- **Render PS1** — Vertex snapping (jitter), affine texture mapping (warp), sem filtro anisotrópico, resolução interna 320×240 ou 480×270 upscalada; dithering ordenado 4×4.
- **Pós-processo VHS** — Scanlines, chroma bleed, ruído de fita e erros de tracking nos momentos de transição de realidade (intensidade configurável).
- **Animação "marionete"** — 12 fps interpolados nos NPCs; as crianças piscam em sincronia (detalhe de fundo perturbador). **A Sininho monstro anima a 24 fps** — ela é a única coisa "fluida" do mundo, o que a torna errada.

### 10.2 Paleta por ambiente

| Ambiente | Paleta / luz | Intenção |
|---|---|---|
| Quarto real | Azul-noturno, âmbar da fresta da porta | Frio e pequeno; a única luz vem "dos adultos" |
| Terra do Nunca (dia) | Dourado saturado, verdes de conto de fadas, céu turquesa | Saturação levemente ALTA demais: bonito como propaganda antiga |
| Terra do Nunca (noite) | Roxos dessaturados, névoa, lua fria | O mesmo mapa com a cor "arrancada" — perda visível do encanto |
| Casa (noite) | Madeira escura, velas quentes e esparsas, breu real entre elas | Ilhas de luz num oceano de escuro; a luz atrai e denuncia |
| Porão | Verde-doentio do gerador, ferrugem | A verdade da ilha: cor de instituição, não de conto |

### 10.3 Direção de personagens

- **Wendy em primeira pessoa: NADA do corpo aparece** — sem mãos, sem braços. Itens equipados "levitam" diante da câmera, no padrão Puppet Combo. **A mão da criança só é vista UMA vez**, na cutscene final do penhasco — raridade que torna o corte insuportavelmente íntimo.
- **Crianças:** 4 modelos-base × paletas de pijama; rostos de textura pintada com sorrisos IGUAIS (uniformidade como horror).
- **Sininho fada:** luz âmbar com bloom suave — **único bloom do jogo**; quando ela vira monstro, o bloom morre para sempre.
- **Sininho monstro:** textura de porcelana rachada; asas como vitral quebrado arrastando; sino escurecido de tamanho absurdo.
- **Peter Pan:** o modelo mais "limpo" e simétrico do jogo, sombra própria levemente dessincronizada da malha (bug proposital).
- **Peter Pan forma dark:** NÃO é um segundo modelo trocado em cena — é o mesmo mesh com um "shader de verdade" que só se manifesta em vislumbres. Recursos: blend shape que alonga silhueta e mãos; segunda mandíbula de dentes revelada só por 1–2 frames; reflexo/sombra usando uma malha alternativa (a forma dark) enquanto o corpo visível continua o menino; camada de voz grave no mixer disparada junto. A malha alternativa aparece por inteiro **apenas na cutscene do penhasco**.

### 10.4 Iluminação e câmeras de cutscene

- Luzes por vela/lanterna com flicker de 2–4 Hz; sombras duras de baixa resolução (estética PS1, e barato de renderizar).
- Cutscenes com câmeras fixas em ângulos desconfortáveis (contra-plongée nas figuras de autoridade; plongée em Wendy).
- **Momento do penhasco:** câmera lateral fixa, o único plano "de cinema" do jogo — citação direta do enquadramento do curta de referência.

---

## 11 · Direção de Som

**Metade do horror deste jogo é áudio.** Trilha original composta em FL Studio pelo autor; mix pensado para fones.

### 11.1 Música — sistema adaptativo

Quatro estados musicais independentes, com crossfade por camadas (stems), no modelo já validado pelo autor no projeto **Graywater**:

| Estado | Conteúdo musical | Gatilho |
|---|---|---|
| **CALMARIA** (dia) | Caixinha de música em dó maior, celesta, cordas de brinquedo; levemente desafinada (−12 cents) | Fases diurnas |
| **VIGÍLIA** (noite, sem ameaça) | Drones graves, a MESMA melodia da caixinha esticada 8×, sino reverso | Loop noturno padrão |
| **CAÇADA** | Percussão de objetos domésticos, clusters de cordas, o badalo integrado ao compasso | Estado PERSEGUIÇÃO da IA |
| **FINALE** | Coral infantil sem palavras + a oração do Pai-Nosso da mãe, filtrada como fita gasta | Clímax e epílogo |

- **Leitmotiv único:** o tema da caixinha de música **É** o tema do jogo — apresentado como conforto, corrompido como ameaça, redimido no finale.
- **Especificação técnica:** stems WAV 16-bit / 44,1 kHz com pontos de loop sem emenda (padrão de exportação já usado pelo autor).

### 11.2 Som diegético (o verdadeiro HUD)

| Fonte | Função |
|---|---|
| Badalo da Sininho | Posição, distância e andar da ameaça; atenuação e oclusão por paredes (FMOD/Unity Audio Mixer com snapshots) |
| Respiração de Wendy | Fôlego e medo; acelera perto do monstro mesmo escondido |
| Superfícies | Cada material com passos próprios; tábuas soltas com rangido de AVISO antes do estouro |
| Tique-taque | Proximidade do mar/crocodilos; também é o relógio do enigma C — **o mesmo som com dois medos** |
| Casa | Madeira que assenta, vento, um relógio em outro cômodo: cama de ruído que esconde/revela informação |

### 11.3 Vozes

- Direção minimalista: Sininho fada com voz doce levemente "esticada"; **o monstro reutiliza a MESMA gravação** com granular + pitch −800 cents (continuidade audível da personagem).
- Peter Pan: sempre em volume de conversa, mesmo a 20 metros (mix não naturalista — ele soa perto porque é inescapável).
- A oração do prólogo é gravada **uma única vez** e reutilizada em 3 contextos (prólogo, trilha do finale, colecionável VHS) com degradações diferentes.

### 11.4 Silêncio

**Regra de mixagem: pelo menos 20% da noite acontece sem NENHUMA música.** O silêncio é orçado como se fosse um asset — é nele que o badalo funciona.

---

## 12 · Especificações Técnicas

Especificações de implementação na Unity, alinhadas à experiência prévia do autor (Unity 2022.3 LTS, C#, projetos 2D URP anteriores).

### 12.1 Stack

| Camada | Escolha |
|---|---|
| Engine | Unity 2022.3 LTS · pipeline 3D **URP** |
| Linguagem | C# · **assembly definitions por módulo** (Core, AI, Interaction, Audio, UI) |
| Render PS1 | Shader Graph: vertex snapping + affine mapping + fog por vértice; RenderTexture 480×270 → upscale point; pós CRT/dither em fullscreen pass |
| IA | **NavMesh + FSM própria** (enum + classes de estado); waypoints editáveis por ScriptableObject |
| Diálogo | Grafo de nós em ScriptableObject; variável int `lucidez` persistida no save; ramos por limiar; falas de NPC leem lucidez para trocar linha/animação (cap. 5.9) |
| Áudio | Unity Audio Mixer com snapshots por estado musical; avaliar FMOD se a oclusão nativa limitar |
| Save | JSON criptografado leve em `persistentDataPath`; slots automáticos por capítulo |
| Input | **Input System (novo)**, com rebind e suporte a controle desde o dia 1 |
| Versionamento | Git + GitHub (TucanoiDEV) · **Git LFS** para binários · branches por feature |

### 12.2 Arquitetura de cenas

`Boot → MainMenu → Prologue → Island_Day (aditiva: Parque + Casa_Exterior) → House_Night (aditiva por andar) → Island_Climax → Epilogue`

- A casa carrega os 4 níveis (térreo, 1º, sótão, porão) como **sub-cenas aditivas sempre ativas**: a Sininho patrulha andares fora da vista do jogador **de verdade** (nada de fake).
- Ilha de dia e de clímax são a **MESMA cena** com perfis de luz/pós/áudio trocados por ScriptableObject de "Fase".

### 12.3 Metas de performance

| Métrica | Alvo | Nota |
|---|---|---|
| Frame rate | **60 fps em GPU integrada moderna** | Estética PS1 é leve por natureza; não desperdiçar a folga |
| Draw calls | < 300 por frame | Atlas de texturas por ambiente; static batching na casa |
| Memória | < 2 GB RAM | Texturas minúsculas ajudam; auditar áudio (maior peso) |
| Build | < 1,5 GB | Compressão Vorbis nos stems de música |
| Loading | < 5 s entre capítulos | Cenas aditivas + tela de estática VHS como máscara |

### 12.4 Riscos técnicos e mitigação

| Risco | Mitigação |
|---|---|
| IA da perseguidora "injusta" ou "burra" | **Prototipar a FSM em greybox ANTES de qualquer arte**; telemetria de capturas no playtest; regras de justiça do cap. 6.1 como testes automatizados |
| Terror que vira frustração (sem armas) | Playtests cegos desde a vertical slice; medir taxa de desistência por trecho; ajustar "período frio" e dicas diegéticas |
| Escopo do mundo aberto | **A ilha é corredor largo cenográfico, não open world**: colisões guiam; cortar zonas antes de encolher a casa |
| Shader PS1 em URP | Existem referências abundantes da comunidade; reservar 1 sprint só para o pipeline visual no início |
| Áudio posicional/oclusão | **Prova de conceito do badalo na semana 1 do protótipo** — é a feature mais importante do jogo |

---

## 13 · Produção e Escopo

Plano de produção realista para desenvolvedor solo com colaborações pontuais, em fases com entregáveis verificáveis.

### 13.1 Fases e marcos

| Fase | Entregável | Duração estimada |
|---|---|---|
| **0 · Pré-produção** | Este GDD aprovado; protótipo de papel dos enigmas; moodboard/blockout da casa | 2–3 semanas |
| **1 · Protótipo** | Greybox do térreo + FSM da Sininho + ruído/esconderijo + badalo posicional. **SEM ARTE.** Meta: "já dá medo?" | 4–6 semanas |
| **2 · Vertical slice** | Primeiro trecho da noite completo (revelação + enigmas A e B) com arte final (pipeline PS1), save, menu. Base do primeiro trailer | 6–8 semanas |
| **3 · Produção** | Noite completa, dia, ilha, clímax; trilha completa; colecionáveis | 4–6 meses |
| **4 · Alpha → Beta** | Conteúdo completo; playtests fechados; passe de acessibilidade; localização PT-BR/EN | 6–8 semanas |
| **5 · Lançamento** | Steam page 3+ meses antes; demo (prólogo + início da noite) no Next Fest; keys para streamers de horror | — |

### 13.2 Ordem de corte (se o escopo apertar)

Cortes pré-decididos, do primeiro ao último — **o núcleo é intocável**:

1º: Final secreto (8.3) · 2º: Fitas VHS colecionáveis · 3º: Modo Pesadelo · 4º: Dias 2–4 viram vinhetas não jogáveis · 5º: Sótão (fundir enigma ao 1º andar).

> **NUNCA CORTAR:** prólogo, transformação da Sininho, mecânica de fechar os olhos, cena do penhasco e epílogo.

### 13.3 Métricas de sucesso do design

- **Playtest:** ≥70% dos jogadores completam a primeira run sem abandonar; tempo médio entre **35 e 50 min**.
- **Nenhum** playtester descreve a Sininho como "aleatória" ou "injusta" após os primeiros 10 minutos de caçada.
- **Streamers:** reações fortes espontâneas ao longo de toda a run (métrica informal, mas é o mercado-alvo).

---

## Apêndices

### A. Glossário do projeto

| Termo | Definição |
|---|---|
| **Badalo** | Som-assinatura da Sininho monstro; principal canal de informação de ameaça |
| **Interação lenta** | Segurar o botão de interagir para agir sem ruído |
| **A Lista** | Documento-âncora da narrativa ambiental: nomes de crianças, riscados |
| **Mapa de calor** | Memória da IA sobre áreas com histórico de ruído do jogador |
| **Lucidez (L)** | Contador oculto de resistência à fantasia; sobe com falas de Lucidez e muda a encenação dos NPCs (cap. 5.9) |
| **Fala de entrega / de Lucidez** | As duas naturezas de escolha de diálogo: aceitar ou resistir à Terra do Nunca |
| **A Noite** | O ato principal do jogo: fuga linear e contínua pela casa, sem cortes |
| **Açougue** | Ala opcional do porão (4.5): a verdade da ilha e o facão |
| **Período frio** | 20 s sem perseguição após recarregar um checkpoint (regra de justiça) |
| **Tranca tripla** | Fechadura de 3 componentes da saída da casa (objetivo macro) |

### B. Referências e inspirações declaradas

- **Puppet Combo** — direção estética PS1/VHS e ritmo de perseguição (referência de linguagem, sem uso de assets).
- **Granny (DVloper)** — referência de lógica dos enigmas de casa-prisão e da audição do inimigo (não da estrutura).
- **Silent Hill 2 (Team Silent)** — estrutura documental deste GDD e horror como metáfora.
- **Resident Evil 2/3 Remake** — design de perseguidor persistente.
- **Outlast (Red Barrels)** — impotência total sem combate e terror de fuga com fonte de luz limitada.
- **Peter Pan – La Obscura Verdad** — tom da releitura e enquadramento do clímax.
- **J. M. Barrie, Peter and Wendy (1911, domínio público)** — mitologia-base: Terra do Nunca, Peter, Sininho, o crocodilo e o relógio.

### C. Roteiro de diálogo e Lucidez (amostra)

Base de escrita do sistema do cap. 5.9. Falas marcadas **[L]** somam +1 de Lucidez; falas **[E]** são de entrega (sem efeito). Ramos entre colchetes com condição (ex.: `[se L≥2]`) só aparecem no limiar indicado. Este é o núcleo canônico do Dia — o roteiro completo é mantido em planilha externa ligada ao grafo de diálogo.

**› Encontro com Sininho (despertar na ilha)**

| Fala / escolha | Natureza e efeito |
|---|---|
| Sininho: "Você finalmente acordou!" | Fala fixa de NPC |
| › "Quem é você?" | **[E]** Curiosidade neutra |
| › "Onde estou?" | **[L]** Primeira faísca de estranhamento (+1) |
| Sininho: "Sou a fada mais especial deste lugar, Sininho! Estamos no lugar mais mágico do universo!" | Fala fixa; tom muda se L já ≥ 1 |

**› Apresentação de Peter Pan (a roda das crianças)**

| Fala / escolha | Natureza e efeito |
|---|---|
| Peter Pan: "Ora ora, se não é o Wendy. Estávamos esperando por você!" | Fala fixa de NPC |
| › "Quem é você?" | **[E]** Neutra |
| › "Como sabe meu nome?" | **[L]** Desconfiança (+1); Peter desconversa ("todos aqui te conhecem, você é especial") |
| Peter Pan: "Que tal apresentarmos os brinquedos? Vamos, me acompanhe!" | Transição para o tutorial |

**› A lista de brinquedos (fim do passeio)**

| Fala / escolha | Natureza e efeito |
|---|---|
| Peter Pan: "Brinque em cada um da lista, apenas uma vez!" | Objetivo do tutorial |
| › "Ebaa, não posso esperar para me divertir!" | **[E]** Wendy entra na fantasia |
| › "Mas eu não quero brincar…" | **[L]** Recusa (+1); Peter: "Para uma criança, TODO momento é hora de brincar." |

**› A ordem de dormir**

| Fala / escolha | Natureza e efeito |
|---|---|
| Peter Pan: "Completou a lista? Muito bem. Já está na hora da cama." | Fecha o Dia |
| › `[se L≥2]` "Sinto saudade da minha mãe." | **[L]** Ramo especial (+1); Peter endurece — a máscara escorrega pela primeira vez |
| › "Tá bom." | **[E]** Obediência; transição direta para o quarto |
| Peter Pan: "Seja bem-vindo ao seu novo lar." | Última fala antes da noite; entonação varia com L |

> **Nota de continuidade:** a Lucidez acumulada no Dia não muda a estrutura da noite de fuga (idêntica em qualquer L), apenas o quanto Peter Pan e Sininho já haviam deixado a verdade transparecer antes dela. É caracterização e recompensa de atenção — nunca um gate de progressão nem um modificador de dificuldade.

### D. Nota final

Este documento é vivo: cada sistema aqui descrito deve ser validado em protótipo antes de virar verdade. O que não pode mudar está escrito no cap. 13.2 — todo o resto existe para servir ao menino que fecha os olhos para sobreviver e precisa abri-los para escapar.

— Tucano · TEREJACKS · 2026
