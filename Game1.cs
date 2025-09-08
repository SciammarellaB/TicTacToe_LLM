using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TicTacToe_Ai;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    //TEXTURES
    Texture2D linhaTextura;

    //MAPA
    int width = 400;
    int height = 400;
    public int[,] map = new int[3,3];
    public Tuple<int, int, string> ultimasJogadas;

    //JOGADORES
    public int player1 = 1;
    public int player2 = 2;
    public int jogadorAtual = 1;

    //CHAT
    public List<ChatModel> mensagens = new List<ChatModel>();

    //HISTÓRICO
    int numeroJogo = 1;
    bool gameOver = false;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here
        SetFrameRate(15);
        ChangeScreenResolution(400, 400);
        numeroJogo = 1;

        //INICIALIZAR IA
        mensagens.Add(new ChatModel("system", "Voce esta numa partida de jogo da velha jogando como o Jogador 2."));
        mensagens.Add(new ChatModel("system", "O tabuleiro e dividido em uma matriz no formato 3x3, onde cada casa e representada por coordenadas (linha,coluna) que vao de 0 a 2."));
        mensagens.Add(new ChatModel("system", "Para ganhar o jogo, e necessario alinhar tres jogadas em uma linha, coluna ou diagonal. Como estamos numa matriz 3x3, o alinhamento pode acontecer (0,0),(0,1),(0,2) ou (1,0),(1,1),(1,2) ou (2,0),(2,1),(2,2) ou (0,0),(1,0),(2,0) ou (0,1),(1,1),(2,1) ou (0,2),(1,2),(2,2) ou (0,0),(1,1),(2,2) ou (0,2),(1,1),(2,0)."));
        mensagens.Add(new ChatModel("system", "Um dos pontos do jogo e que voce deve tentar ganhar, mas tambem impedir que o adversario ganhe. Portanto, algumas jogadas podem ser para impedir que o adversario consiga alinhar tres simbolos. Isso acontece quando o adversario ja tem dois simbolos alinhados e voce precisa jogar na terceira casa para bloquear a vitoria dele."));
        mensagens.Add(new ChatModel("system", "O jogo pode terminar empatado, caso todas as casas do tabuleiro sejam preenchidas sem que nenhum dos jogadores tenha conseguido alinhar tres jogadas."));
        mensagens.Add(new ChatModel("system", "Sera mantido um historico das partidas anteriores, para que voce possa analisar as jogadas feitas e aprender com elas."));
        mensagens.Add(new ChatModel("system", "Voce nao pode jogar em uma casa que ja tenha sido escolhida pelos jogadores."));
        mensagens.Add(new ChatModel("system", "Se nao houver jogadas possiveis, jogue na primeira casa vazia que encontrar, seguindo a ordem da matriz (0,0) ate (2,2)."));
        mensagens.Add(new ChatModel("system", $"Responda apenas com a coordenada (linha,coluna) da sua jogada. Exemplo: 0,0"));
        mensagens.Add(new ChatModel("system", $"Não é necessário retornar o tabuleiro atual. Apenas a sua resposta com a jogada."));

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // TODO: use this.Content to load your game content here
        linhaTextura = Content.Load<Texture2D>("10x10");
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // TODO: Add your update logic here
        if (!gameOver)
        {
            var casa = ObterCasaMouse(Mouse.GetState().X, Mouse.GetState().Y);

            if (jogadorAtual == player1)
            {
                if (Mouse.GetState().LeftButton == ButtonState.Pressed && casa.Item1 != -1 && casa.Item2 != -1)
                {
                    GravarJogada(casa.Item1, casa.Item2, player1);
                    ultimasJogadas = new Tuple<int, int, string>(casa.Item1, casa.Item2, "Jogador 1");
                    mensagens.Add(new ChatModel("user", $"Jogador 1 jogou na casa {casa.Item1},{casa.Item2}"));
                    jogadorAtual = player2;

                    TestarGanhador();
                }
            }
            else
            {
                var jogadaIA = JogadaIA().GetAwaiter().GetResult();
                GravarJogada(jogadaIA.Item1, jogadaIA.Item2, player2);
                ultimasJogadas = new Tuple<int, int, string>(jogadaIA.Item1, jogadaIA.Item2, "Jogador 2");
                mensagens.Add(new ChatModel("user", $"Jogador 2 jogou na casa {jogadaIA.Item1},{jogadaIA.Item2}"));
                jogadorAtual = player1;

                TestarGanhador();
            }
        }
        else
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Space))
            {
                ResetarJogo();
                gameOver = false;
                numeroJogo++;
            }
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        // TODO: Add your drawing code here
        _spriteBatch.Begin();

        var w = width / 3;
        var h = height / 3;

        #region DESENHA JOGADAS
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                var x = i * w;
                var y = j * h;
                var spot = map[j, i];

                if (spot == player1)
                    _spriteBatch.DrawString(Content.Load<SpriteFont>("gameFont"), "X", new Vector2(x + w / 2 - 24, y + h / 2 - 48), Color.Black);
                else if (spot == player2)
                    _spriteBatch.DrawString(Content.Load<SpriteFont>("gameFont"), "O", new Vector2(x + w / 2 - 24, y + h / 2 - 48), Color.Red);
            }
        }
        #endregion

        #region DESENHA LINHAS
        //LINHAS HORIZONTAIS
        _spriteBatch.Draw(linhaTextura, new Rectangle(0, h, width, 10), Color.Black);
        _spriteBatch.Draw(linhaTextura, new Rectangle(0, h * 2, width, 10), Color.Black);
        //LINHAS VERTICAIS
        _spriteBatch.Draw(linhaTextura, new Rectangle(w, 0, 10, height), Color.Black);
        _spriteBatch.Draw(linhaTextura, new Rectangle(w * 2, 0, 10, height), Color.Black);
        #endregion

        _spriteBatch.End();

        base.Draw(gameTime);
    }

    #region METODOS
    public void SetFrameRate(int fps)
    {
        IsFixedTimeStep = true;
        TargetElapsedTime = System.TimeSpan.FromSeconds(1.0 / fps);
    }
    public void ChangeScreenResolution(int width, int height)
    {
        _graphics.PreferredBackBufferWidth = width;
        _graphics.PreferredBackBufferHeight = height;
        _graphics.ApplyChanges();
    }
    public Tuple<int, int> ObterCasaMouse(int mouseX, int mouseY)
    {
        var w = width / 3;
        var h = height / 3;

        // Verifica se o mouse está dentro da janela
        if (mouseX < 0 || mouseX >= width || mouseY < 0 || mouseY >= height)
        {
            return new Tuple<int, int>(-1, -1);
        }
        var coluna = (int)Math.Round((double)(mouseX / w), MidpointRounding.AwayFromZero);
        var linha = (int)Math.Round((double)(mouseY / h), MidpointRounding.AwayFromZero);

        return new Tuple<int, int>(linha, coluna);
    }
    public void GravarJogada(int linha, int coluna, int player)
    {
        if (map[linha, coluna] == 0)
            map[linha, coluna] = player;
    }
    public void TestarGanhador()
    {
        //LINHAS
        for (int i = 0; i < 3; i++)
        {
            if (map[i, 0] == player1 && map[i, 1] == player1 && map[i, 2] == player1)
            {
                gameOver = true;
                Console.WriteLine("Player 1 Venceu");
                mensagens.Add(new ChatModel("agent", "Player 1 Venceu"));
                return;
            }
            if (map[i, 0] == player2 && map[i, 1] == player2 && map[i, 2] == player2)
            {
                gameOver = true;
                Console.WriteLine("Player 2 Venceu");
                mensagens.Add(new ChatModel("agent", "Player 2 Venceu"));
                return;
            }
        }
        //COLUNAS
        for (int i = 0; i < 3; i++)
        {
            if (map[0, i] == player1 && map[1, i] == player1 && map[2, i] == player1)
            {
                gameOver = true;
                Console.WriteLine("Player 1 Venceu");
                mensagens.Add(new ChatModel("agent", "Player 1 Venceu"));
                return;
            }
            if (map[0, i] == player2 && map[1, i] == player2 && map[2, i] == player2)
            {
                gameOver = true;
                Console.WriteLine("Player 2 Venceu");
                mensagens.Add(new ChatModel("agent", "Player 2 Venceu"));
                return;
            }
        }
        //DIAGONAIS
        if (map[0, 0] == player1 && map[1, 1] == player1 && map[2, 2] == player1)
        {
            gameOver = true;
            Console.WriteLine("Player 1 Venceu");
            mensagens.Add(new ChatModel("agent", "Player 1 Venceu"));
            return;
        }
        if (map[0, 0] == player2 && map[1, 1] == player2 && map[2, 2] == player2)
        {
            gameOver = true;
            Console.WriteLine("Player 2 Venceu");
            mensagens.Add(new ChatModel("agent", "Player 2 Venceu"));
            return;
        }
        if (map[0, 2] == player1 && map[1, 1] == player1 && map[2, 0] == player1)
        {
            gameOver = true;
            Console.WriteLine("Player 1 Venceu");
            mensagens.Add(new ChatModel("agent", "Player 1 Venceu"));
            return;
        }
        if (map[0, 2] == player2 && map[1, 1] == player2 && map[2, 0] == player2)
        {
            gameOver = true;
            Console.WriteLine("Player 2 Venceu");
            mensagens.Add(new ChatModel("agent", "Player 2 Venceu"));
            return;
        }

        if (MapaCompleto())
        {
            gameOver = true;
            Console.WriteLine("Empate");
            mensagens.Add(new ChatModel("agent", "Empate"));
            return;
        }
    }
    public void ResetarJogo()
    {
        map = new int[3, 3];
        gameOver = false;
        mensagens.RemoveRange(8, mensagens.Count - 8); // Mantém apenas as mensagens iniciais do sistema
        jogadorAtual = player1;
    }
    public bool MapaCompleto()
    {
        for (int i = 0; i < 3; i++)
            for (int j = 0; j < 3; j++)
                if (map[i, j] == 0)
                    return false;
        return true;
    }
    #endregion

    #region OLLAMA
    public async Task<Tuple<int, int>> JogadaIA()
    {
        using var httpClient = new HttpClient();
        var requestBody = new
        {
            model = "llama3.1",
            stream = false,
            options = new
            {
                temperature = 0,
                top_p = 0.9,
                max_new_tokens = 10
            },
            messages = mensagens
        };

        var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(requestBody), System.Text.Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync("http://localhost:11434/api/chat", content);
        response.EnsureSuccessStatusCode();
        var responseString = await response.Content.ReadAsStringAsync();
        System.Console.WriteLine($"{responseString}");
        var match = System.Text.RegularExpressions.Regex.Match(responseString, @"\b[0-2],[0-2]\b");
        if (match.Success)
        {
            var parts = match.Value.Split(',');
            int linha = int.Parse(parts[0]);
            int coluna = int.Parse(parts[1]);

            // Só grava jogada se a casa estiver vazia
            if (map[linha, coluna] == 0)
            {
                return new Tuple<int, int>(linha, coluna);
            }
            else
            {
                System.Console.WriteLine($"Jogada inválida sugerida pela IA: {linha},{coluna}");
                mensagens.Add(new ChatModel("user", $"Jogador 2 fez uma jogada inválida: {linha},{coluna}"));
                return await JogadaIA();
            }
        }
        else
        {
            System.Console.WriteLine("Nenhuma jogada possível");
            return await JogadaIA();    
            // return new Tuple<int, int>(-1, -1);
        }
    }
    #endregion
}


public class ChatModel
{
    public string role { get; set; }
    public string content { get; set; }

    public ChatModel()
    {

    }

    public ChatModel(string role, string content)
    {
        this.role = role;
        this.content = content;
    }
}