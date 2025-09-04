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

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here
        ChangeScreenResolution(width, height);
        SetFrameRate(15);
        base.Initialize();

        //INICIALIZAR IA
        mensagens.Add(new ChatModel("system", "Você está numa partida de jogo da velha."));
        mensagens.Add(new ChatModel("system", "O tabuleiro é uma matriz 3x3, onde as linhas e colunas são numeradas de 0 a 2."));
        mensagens.Add(new ChatModel("system", "Você não pode jogar em uma casa que já tenha sido escolhida."));
        mensagens.Add(new ChatModel("system", "Verifique na conversa se a casa que você deseja jogar já não foi escolhida."));
        mensagens.Add(new ChatModel("system", "Para ganhar o jogo, você deve alinhar 3 símbolos iguais na horizontal, vertical ou diagonal."));
        mensagens.Add(new ChatModel("system", "O retorno da sua jogada deverá ser apenas os números das coordenadas da matriz (linha,coluna) que deseja jogar."));
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

        TestarGanhador();

        if (MapaCompleto())
        {
            System.Console.WriteLine("Empate");
            ResetarMapa();
            jogadorAtual = player1;
        }

        var casa = ObterCasaMouse(Mouse.GetState().X, Mouse.GetState().Y);

        if (jogadorAtual == player1)
        {
            if (Mouse.GetState().LeftButton == ButtonState.Pressed && casa.Item1 != -1 && casa.Item2 != -1)
            {
                GravarJogada(casa.Item1, casa.Item2, player1);
                ultimasJogadas = new Tuple<int, int, string>(casa.Item1, casa.Item2, "Jogador 1");
                mensagens.Add(new ChatModel("user", $"Jogador 1 jogou na casa {casa.Item1},{casa.Item2}"));
                jogadorAtual = player2;
            }

        }
        else
        {
            var jogadaIA = JogadaIA().GetAwaiter().GetResult();
            GravarJogada(jogadaIA.Item1, jogadaIA.Item2, player2);
            ultimasJogadas = new Tuple<int, int, string>(jogadaIA.Item1, jogadaIA.Item2, "Jogador 2");
            mensagens.Add(new ChatModel("user", $"Jogador 2 jogou na casa {jogadaIA.Item1},{jogadaIA.Item2}"));
            jogadorAtual = player1;
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
        var coluna = mouseX / w;
        var linha = mouseY / h;
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
            if (map[i,0] == player1 && map[i,1] == player1 && map[i,2] == player1)
                System.Console.WriteLine("Player 1 Venceu");
            if (map[i,0] == player2 && map[i,1] == player2 && map[i,2] == player2)
                System.Console.WriteLine("Player 2 Venceu");
        }
        //COLUNAS
        for (int i = 0; i < 3; i++)
        {
            if (map[0,i] == player1 && map[1,i] == player1 && map[2,i] == player1)
                System.Console.WriteLine("Player 1 Venceu");
            if (map[0,i] == player2 && map[1,i] == player2 && map[2,i] == player2)
                System.Console.WriteLine("Player 2 Venceu");
        }
        //DIAGONAIS
        if (map[0,0] == player1 && map[1,1] == player1 && map[2,2] == player1)
            System.Console.WriteLine("Player 1 Venceu");
        if (map[0,0] == player2 && map[1,1] == player2 && map[2,2] == player2)
            System.Console.WriteLine("Player 2 Venceu");

        if (map[0,2] == player1 && map[1,1] == player1 && map[2,0] == player1)
            System.Console.WriteLine("Player 1 Venceu");
        if (map[0,2] == player2 && map[1,1] == player2 && map[2,0] == player2)
            System.Console.WriteLine("Player 2 Venceu");
    }
    public void ResetarMapa()
    {
        map = new int[3,3];
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
                temperature = 0
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
                return new Tuple<int, int>(-1, -1);
            }
        }
        else
        {
            System.Console.WriteLine("Nenhuma jogada possível");
            return new Tuple<int, int>(-1, -1);
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