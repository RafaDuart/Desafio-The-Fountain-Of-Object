

using static PlayerInput;

new FoutainOfObjectsGame().Run();

public class FoutainOfObjectsGame
{
    public Player Player { get; }
    public Mapa Mapa { get; }

    public FoutainOfObjectsGame()
    {
        Player = new Player();
        Mapa = new Mapa();
    }

    public void Run()
    {
        PlayerInput playerInput = new PlayerInput();
        while (!Ganhou())
        {
            SenseStuff();
            IAcao acao = playerInput.EscolhaAcao();
            acao.Execute(this);

        }
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("A Fountain of Objects está ativada, você escapou com vida!");
        Console.WriteLine("Você ganhou!");
    }

    private bool Ganhou()
    {
        Sala playersSala = Mapa.GetSala(Player.Localizacao);
        if (playersSala is not EntradaSala) return false;

        for (int linha = 0; linha < Mapa.Linhas; linha++)
            for (int coluna = 0; coluna < Mapa.Colunas; coluna++)
                if (Mapa.GetSala(linha, coluna) is FountainSala fountainSala)
                    if (!fountainSala.Desligado)
                        return false;
        return true;

    }

    private void SenseStuff()
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("------------------------------------------------");
        Console.WriteLine($"Você está em uma sala (Linha={Player.Localizacao.Linha}, Coluna={Player.Localizacao.Coluna}).");

        SenseEntradaSalaLuz();
        SenseFountainSom();
    }

    private void SenseFountainSom()
    {
        if (Mapa.GetSala(Player.Localizacao) is FountainSala fountainSala)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            if (fountainSala.Ligado) Console.WriteLine("Você ouve as águas correntes da vindo da Fountain of Objects. ");
            else Console.WriteLine("Você ouve o respingar da água nessa sala. A Fountain Of Objects está aqui!");
        }
          
    }
    private void SenseEntradaSalaLuz()
    {
        Console.ForegroundColor = ConsoleColor.Yellow; 
        if (Mapa.GetSala(Player.Localizacao) is EntradaSala)
            Console.WriteLine("Você vê uma luz vindo da entrada da caverna");
    }
}

public interface IAcao
{
    void Execute(FoutainOfObjectsGame game);
}

public enum Direcao { Norte, Sul, Leste, Oeste }

public class AcaoMovimentacao : IAcao
{
    private readonly Direcao _direcao;
    public AcaoMovimentacao(Direcao direcao)
    {
        _direcao = direcao;
    }

    public void Execute(FoutainOfObjectsGame game)
    {
        Localizacao atual = game.Player.Localizacao;
        Localizacao proxima = GetLocalizacaoAtual(atual, _direcao);
        if (game.Mapa.IsOnMapa(proxima))
            game.Player.Localizacao = proxima;
        else
            Console.WriteLine("Você não pode se mover para está direção !");

    }

    private Localizacao GetLocalizacaoAtual(Localizacao comeco, Direcao MoverDirecao)
    {
        return MoverDirecao switch
        {
            Direcao.Norte => new Localizacao(comeco.Linha - 1, comeco.Coluna),
            Direcao.Sul => new Localizacao(comeco.Linha + 1, comeco.Coluna),
            Direcao.Leste => new Localizacao(comeco.Linha, comeco.Coluna + 1),
            Direcao.Oeste => new Localizacao(comeco.Linha, comeco.Coluna - 1)
        };
    }
}

public class AtivarFoutainAcao : IAcao
{
    public void Execute(FoutainOfObjectsGame game)
    {
        Localizacao playersLocalizacao = game.Player.Localizacao;
        Sala playersSala = game.Mapa.GetSala(playersLocalizacao);
        FountainSala? fountainSala = playersSala as FountainSala;
        if (fountainSala == null)
        {
            Console.WriteLine("Você não pode fazer isso nesta sala");
            return;
        }
        fountainSala.Ligado = true;
    }

}

public class PlayerInput
{
    public IAcao EscolhaAcao()
    {
        do
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Oque você quer fazer ?");
            Console.ForegroundColor = ConsoleColor.Cyan;
            string? input = Console.ReadLine();
            IAcao? EscolhaAcao = input switch
            {
                "Se Mover ao Norte" => new AcaoMovimentacao(Direcao.Norte),
                "Se Mover ao Sul" => new AcaoMovimentacao(Direcao.Sul),
                "Se Mover ao Leste" => new AcaoMovimentacao(Direcao.Leste),
                "Se Mover ao Oeste" => new AcaoMovimentacao(Direcao.Oeste),
                "Ativar Fountain" => new AtivarFoutainAcao(),
                _                   => null
            };
            if (EscolhaAcao != null) return EscolhaAcao;

            Console.WriteLine("Eu desconheço deste comando. ");
        }
        while (true);
    }

    public class Player
    {

        public Localizacao Localizacao { get; set; } = new Localizacao(0, 0);

    }

    public record Localizacao(int Linha, int Coluna);

    public class Mapa
    {
        private readonly Sala[,] _salas;

        public Mapa()
        {
            _salas = new Sala[Linhas, Colunas];
            for (int linha = 0; linha < Linhas; linha++)
                for (int coluna = 0; coluna < Colunas; coluna++)
                    _salas[linha, coluna] = new SalaVazia();

            _salas[0, 0] = new EntradaSala();
            _salas[0, 2] = new FountainSala();
        }
        public int Linhas => 4;
        public int Colunas => 4;
        public bool IsOnMapa(Localizacao localizacao)
        {
            if (localizacao.Linha < 0) return false;
            if (localizacao.Linha >= Linhas) return false;
            if (localizacao.Coluna < 0) return false;
            if (localizacao.Coluna >= Colunas) return false;
            return true;
        }

        public Sala GetSala(int linha, int coluna) => _salas[linha, coluna];
        public Sala GetSala(Localizacao localizacao) => _salas[localizacao.Linha, localizacao.Coluna];

    }


    public abstract class Sala { }
    public class SalaVazia : Sala { }
    public class EntradaSala : Sala { }
    public class FountainSala : Sala
    {
        public bool Ligado { get; set; }
        public bool Desligado => !Ligado;
    }
}