# Laboratório de Empuxo 2D

Versão melhorada do projeto Unity com uma experiência jogável sobre densidade e flutuação.

## Como jogar

1. Abra o projeto pelo Unity Hub.
2. Abra `Assets/Scenes/SampleScene.unity`.
3. Pressione **Play**.
4. Use **A/D** ou as **setas laterais** para andar, **W/seta para cima/Espaço** para subir na água, **S/seta para baixo** para mergulhar, **E** para pegar e **R** para reiniciar.

Empurre os três objetos para o lago:

- **Baú — densidade 2,35:** afunda.
- **Barril — densidade 0,92:** permanece quase em equilíbrio e flutua baixo.
- **Garrafa — densidade 0,22:** sobe rapidamente e boia na superfície.

Depois que cada objeto entrar na água, nade até ele e pressione **E** para recuperá-lo. A fase termina quando os três forem recolhidos. Há uma escadaria submersa no lado esquerdo para voltar à margem.

## Conteúdo novo

- Personagem original animado em pixel art.
- Baú, barril e garrafa originais em pixel art.
- Cenário noturno de lago e floresta.
- Sistema de empuxo baseado na fração submersa e densidade relativa.
- Água com arrasto linear e angular.
- Interface em português, rótulos dos objetos e instruções.
- Iluminação visual, água translúcida e vagalumes animados.

Os novos scripts estão em `Assets/Scripts/BuoyancyLab` e os novos assets em `Assets/Resources/Art`.
