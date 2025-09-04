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

    //JOGADORES
    public int player1 = 1;
    public int player2 = 2;
    public int jogadorAtual = 1;

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

        ObterCasaMouse(Mouse.GetState().X, Mouse.GetState().Y, out int linha, out int coluna);

        if (jogadorAtual == player1)
        {
            if (Mouse.GetState().LeftButton == ButtonState.Pressed && linha != -1 && coluna != -1)
            {
                GravarJogada(linha, coluna, player1);
                jogadorAtual = player2;
            }

        }
        else
        {
            JogadaIA(false).GetAwaiter().GetResult();
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
    public void ObterCasaMouse(int mouseX, int mouseY, out int linha, out int coluna)
    {
        var w = width / 3;
        var h = height / 3;

        // Verifica se o mouse está dentro da janela
        if (mouseX < 0 || mouseX >= width || mouseY < 0 || mouseY >= height)
        {
            linha = -1;
            coluna = -1;
            return;
        }

        coluna = mouseX / w;
        linha = mouseY / h;
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
    public async Task JogadaIA(bool invalida)
    {
        using var httpClient = new HttpClient();
        var prompt = "Você está numa partida de jogo da velha, você é o jogador 2. " +
            "O tabuleiro está representado por 3 linhas e 3 colunas. " +
            "Cada célula pode estar vazia (0), ocupada pelo jogador 1 ou pelo jogador 2. " +
            "Jogue apenas em células com 0. Não jogue em casas já ocupadas (com 1 ou 2). " +
            "Se não houver jogadas possíveis, responda com 'Nenhuma jogada possível'. " +
            "Responda apenas com a coordenada que deseja jogar sem nenhum texto extra, no formato 'linha,coluna' Exemplo: 1,0. " +
            "Aqui está o estado atual do tabuleiro:" +
            $"{map[0,0]},{map[0,1]},{map[0,2]} / " +
            $"{map[1,0]},{map[1,1]},{map[1,2]} / " +
            $"{map[2,0]},{map[2,1]},{map[2,2]} / ";

        if (invalida)
        {
            prompt += "A jogada anterior foi inválida pois a casa já estava ocupada.";
        }

        var requestBody = new
        {
            model = "llama3.1",
            options = new
            {
                // temperature = 0.2
            },
            prompt = prompt,
            stream = false
        };
        var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(requestBody), System.Text.Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync("http://localhost:11434/api/generate", content);
        response.EnsureSuccessStatusCode();
        var responseString = await response.Content.ReadAsStringAsync();
        var match = System.Text.RegularExpressions.Regex.Match(responseString, @"\b[0-2],[0-2]\b");
        if (match.Success)
        {
            var parts = match.Value.Split(',');
            int linha = int.Parse(parts[0]);
            int coluna = int.Parse(parts[1]);

            // Só grava jogada se a casa estiver vazia
            if (map[linha, coluna] == 0)
                GravarJogada(linha, coluna, player2);
            else
            {
                System.Console.WriteLine($"Jogada inválida sugerida pela IA: {linha},{coluna}");
                // await JogadaIA(true);
            }
        }
        else
        {
            System.Console.WriteLine("Nenhuma jogada possível");
        }
        System.Console.WriteLine(responseString);
    }
    #endregion
}
