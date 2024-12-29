using System;
using System.Collections.Generic;
using System.Linq;

public class Character
{
    public string Name { get; set; }
    public int Health { get; set; } = 100;
    public int Attack { get; set; } = 10;
    public List<string> Inventory { get; set; } = new List<string>();

    public void AddItem(string item)
    {
        Inventory.Add(item);
        Console.WriteLine($"Вы подобрали {item}.");
    }
    public void RemoveItem(string item)
    {
        Inventory.Remove(item);
        Console.WriteLine($"Вы выкинули {item}.");
    }
    public bool HasItem(string item)
    {
        return Inventory.Contains(item);
    }
    public void TakeDamage(int damage)
    {
        Health -= damage;
        Console.WriteLine($"Вы получили {damage} урона. Ваше здоровье: {Health}");
        if (Health <= 0)
        {
            Console.WriteLine("Вы погибли...");
        }
    }
    public void Heal(int healAmount)
    {
        Health += healAmount;
        Console.WriteLine($"Ваше здоровье востановлено на {healAmount}. Ваше здоровье: {Health}");
    }
}

public class Enemy
{
    public string Name { get; set; }
    public int Health { get; set; }
    public int Damage { get; set; }
    public int Attack { get; set; }
    public Enemy(string name, int health, int damage, int attack)
    {
        Name = name;
        Health = health;
        Damage = damage;
        Attack = attack;
    }
    public void TakeDamage(int damage)
    {
        Health -= damage;
        Console.WriteLine($"Враг {Name} получил {damage} урона. Его здоровье: {Health}");
        if (Health <= 0)
        {
            Console.WriteLine($"Враг {Name} погиб!");
        }
    }
}

public class Location
{
    public string Name { get; set; }
    public string Description { get; set; }
    public Dictionary<string, Location> Exits { get; set; } = new Dictionary<string, Location>();
    public List<string> Items { get; set; } = new List<string>();
    public Action OnEnter { get; set; }
    public Enemy Enemy { get; set; } = null;

    public void AddExit(string direction, Location location)
    {
        Exits[direction] = location;
    }

    public void AddItem(string item)
    {
        Items.Add(item);
    }
    public void RemoveItem(string item)
    {
        Items.Remove(item);
    }
    public bool HasItem(string item)
    {
        return Items.Contains(item);
    }
}
public class Quest
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Target { get; set; }
    public int Reward { get; set; }
    public bool IsCompleted { get; set; }
}

public class NPC
{
    public string Name { get; set; }
    public Dictionary<string, string> Dialogues { get; set; } = new Dictionary<string, string>();
    public Quest Quest { get; set; }
    public Action OnInteract { get; set; }
    public NPC(string name)
    {
        Name = name;
    }
}

public class Game
{
    private Character player;
    private Location currentLocation;
    private List<Location> locations = new List<Location>();
    private List<Quest> quests = new List<Quest>();
    private List<NPC> npcs = new List<NPC>();
    private Random random = new Random();
    private enum GameState { Travel, Combat, Dialog }
    private GameState currentState = GameState.Travel;
    private Enemy currentEnemy;
    public void StartGame()
    {
        InitializeGame();
        GameLoop();
    }

    private void InitializeGame()
    {
        player = new Character();
        Console.Write("Введите имя вашего персонажа: ");
        player.Name = Console.ReadLine();
        Console.WriteLine($"Добро пожаловать в игру, {player.Name}!");

        // Создание локаций
        Location forest = new Location { Name = "Лес", Description = "Вы находитесь в густом лесу. Деревья стоят стеной, и вы чувствуете себя потерянным." };
        Location clearing = new Location { Name = "Поляна", Description = "Вы вышли на небольшую поляну. В центре стоит старый колодец." };
        Location caveEntrance = new Location { Name = "Вход в пещеру", Description = "Вы стоите перед темным входом в пещеру. Кажется, что внутри очень холодно." };
        Location deepCave = new Location { Name = "Глубина пещеры", Description = "Вы в глубине пещеры. Вокруг камни и сталактиты, а впереди что-то блестит." };
        
        locations.Add(forest);
        locations.Add(clearing);
        locations.Add(caveEntrance);
        locations.Add(deepCave);

        // Добавление предметов
        clearing.AddItem("веревка");
        deepCave.AddItem("золотой кубок");
        deepCave.AddItem("лечебное зелье");
        // Добавление врагов
        forest.Enemy = new Enemy("Дикий кабан", 30, 5, 5);
        caveEntrance.Enemy = new Enemy("Пещерный тролль", 60, 15, 10);
        
        // Добавление NPC
        NPC oldMan = new NPC("Старик");
        oldMan.Dialogues.Add("привет", "Здравствуй, путник! Я старый странник. Нужна ли тебе моя помощь?");
        oldMan.Dialogues.Add("помощь", "Могу дать тебе квест. Ты можешь найти волшебный артефакт в пещере?");
        oldMan.OnInteract = () =>
        {
            if (oldMan.Quest == null)
            {
                Console.WriteLine("Старик: Ты можешь найти волшебный артефакт в пещере?");
                Console.WriteLine("Согласится?");
                string input = Console.ReadLine().ToLower();
                if (input == "да")
                {
                   oldMan.Quest = new Quest() {
                      Name = "Собрать артефакт",
                      Description = "Найти волшебный артефакт в глубине пещеры",
                      Target = "золотой кубок",
                      Reward = 10
                   };
                    quests.Add(oldMan.Quest);
                    Console.WriteLine("Старик: Спасибо!");
                } else {
                  Console.WriteLine("Старик: Ну как знаешь...");
                }
            }
           
        };
        npcs.Add(oldMan);
        clearing.OnEnter = () =>
        {
             if (currentLocation == clearing && npcs.Contains(oldMan))
            {
                Console.WriteLine("На поляне вы встретили старика.");
                 Console.WriteLine("Для взаимодействия используйте 'говорить'");
            }
        };

        // Связываем локации
        forest.AddExit("вперед", clearing);
        forest.AddExit("назад", forest); //Возвращаемся в себя
        clearing.AddExit("назад", forest);
        clearing.AddExit("вперед", caveEntrance);
        caveEntrance.AddExit("назад", clearing);
        caveEntrance.AddExit("вперед", deepCave);
        deepCave.AddExit("назад", caveEntrance);


        // Установка начальной локации
        currentLocation = forest;
        currentLocation.OnEnter = () =>
        {
            if (random.Next(0, 100) < 30)
            {
                Console.WriteLine("В лесу, из кустов, неожиданно выпрыгнул дикий кабан и укусил вас!");
                player.TakeDamage(10);
            }
        };
    }
    private void StartCombat(Enemy enemy)
    {
        Console.WriteLine($"Вы вступили в бой с {enemy.Name}!");
        currentState = GameState.Combat;
        currentEnemy = enemy;
        CombatLoop();
    }
    private void CombatLoop()
    {
        while (player.Health > 0 && currentEnemy.Health > 0)
        {
            Console.WriteLine("\n-- Бой --");
            Console.WriteLine($"Ваше здоровье: {player.Health}, Здоровье врага: {currentEnemy.Health}");
            Console.WriteLine("Выберите действие: атаковать / использовать / бежать");
            string action = Console.ReadLine().ToLower();

            if (action == "атаковать")
            {
                 int playerAttack = player.Attack + random.Next(0,5);
                currentEnemy.TakeDamage(playerAttack);
                 if(currentEnemy.Health <= 0) break;
                int enemyAttack = currentEnemy.Damage + random.Next(0,5);
                player.TakeDamage(enemyAttack);
            }
            else if(action == "использовать") {
                Console.WriteLine("Выберите предмет для использования:");
                Console.WriteLine("Ваш инвентарь: " + (player.Inventory.Count > 0 ? string.Join(", ", player.Inventory) : "пусто"));
                string itemToUse = Console.ReadLine().ToLower();
                if (player.HasItem(itemToUse))
                {
                    if (itemToUse == "лечебное зелье")
                    {
                        player.Heal(20);
                        player.RemoveItem(itemToUse);
                     }
                     else
                     {
                         Console.WriteLine("Этот предмет нельзя использовать в бою.");
                     }
                }
                else
                {
                    Console.WriteLine("У вас нет такого предмета.");
                }
            }
            else if (action == "бежать")
            {
                Console.WriteLine("Вы попытались сбежать.");
                if(random.Next(0,100) < 50)
                {
                     Console.WriteLine("Вам удалось сбежать!");
                     currentState = GameState.Travel;
                     return;
                } else {
                      Console.WriteLine("Вам не удалось сбежать!");
                       int enemyAttack = currentEnemy.Damage + random.Next(0,5);
                        player.TakeDamage(enemyAttack);
                }
                
            }
             else
             {
               Console.WriteLine("Неизвестное действие");
            }
        }
        if(player.Health <= 0) return;
        if (currentEnemy.Health <= 0)
        {
            Console.WriteLine($"Вы победили {currentEnemy.Name}!");
        }
        currentState = GameState.Travel;
    }
    private void StartDialog(NPC npc)
    {
        currentState = GameState.Dialog;
        Console.WriteLine($"Вы заговорили с {npc.Name}.");
       
       
           while (currentState == GameState.Dialog)
            {
                  foreach(var dialogue in npc.Dialogues)
                    {
                        Console.WriteLine($"[{dialogue.Key}] - {dialogue.Value}");
                    }
                    Console.WriteLine("Введите фразу для ответа или 'уйти'");
                  string input = Console.ReadLine().ToLower();
                 
                  if(input == "уйти")
                  {
                      currentState = GameState.Travel;
                      break;
                  }
                  if(npc.Dialogues.ContainsKey(input))
                  {
                        Console.WriteLine(npc.Dialogues[input]);
                        if(npc.OnInteract != null)
                        {
                          npc.OnInteract();
                        }
                  }
                  else {
                      Console.WriteLine("У NPC нет такой фразы");
                  }
            }
      
    }
    private void GameLoop()
    {
        while (player.Health > 0)
        {
            Console.WriteLine();
             if (currentState == GameState.Travel)
            {
                   Console.WriteLine($"Вы находитесь в: {currentLocation.Name}");
                    Console.WriteLine(currentLocation.Description);
                    if (currentLocation.Items.Count > 0)
                    {
                        Console.WriteLine("Вы видите тут: " + string.Join(", ", currentLocation.Items));
                    }
                    Console.WriteLine("Куда вы пойдете?");
                    Console.WriteLine("Доступные направления: " + string.Join(", ", currentLocation.Exits.Keys));
                    Console.WriteLine("Ваш инвентарь: " + (player.Inventory.Count > 0 ? string.Join(", ", player.Inventory) : "пусто"));

                if(currentLocation.OnEnter != null)
                {
                    currentLocation.OnEnter();
                }
                if (currentLocation.Enemy != null)
                {
                   StartCombat(currentLocation.Enemy);
                   if(player.Health <= 0) break;
                   currentLocation.Enemy = null; //Убираем врага после боя
                }
                
                  if(player.Health <= 0)
                  {
                     break;
                   }
                    string input = Console.ReadLine().ToLower();

                    if (currentLocation.Exits.ContainsKey(input))
                    {
                        currentLocation = currentLocation.Exits[input];
                        continue;
                    }
                    if (input.StartsWith("взять "))
                    {
                        string item = input.Substring(6);
                        if (currentLocation.HasItem(item))
                        {
                            currentLocation.RemoveItem(item);
                            player.AddItem(item);
                        }
                        else
                        {
                            Console.WriteLine("Тут нет такого предмета.");
                        }
                        continue;
                    }
                    if (input.StartsWith("выбросить "))
                    {
                        string item = input.Substring(10);
                        if (player.HasItem(item))
                        {
                            currentLocation.AddItem(item);
                            player.RemoveItem(item);
                        }
                        else
                        {
                            Console.WriteLine("У вас нет такого предмета.");
                        }
                        continue;
                    }
                    if (input == "помощь")
                    {
                        Console.WriteLine("Доступные команды: ");
                        Console.WriteLine("   - <направление>, например: вперед, назад");
                        Console.WriteLine("   - взять <предмет>");
                        Console.WriteLine("   - выбросить <предмет>");
                         Console.WriteLine("   - говорить");
                        continue;
                    }
                    if (input == "говорить")
                    {
                         NPC npc = npcs.FirstOrDefault(n => currentLocation.OnEnter == n.OnInteract);
                         if(npc != null)
                           StartDialog(npc);
                         else {
                             Console.WriteLine("Не с кем поговорить");
                         }
                            
                           continue;
                    }

                    Console.WriteLine("Неизвестная команда.");
                }
           
            
        }
        if(player.Health <= 0) {
          Console.WriteLine("Вы проиграли...");
        }
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        Game game = new Game();
        game.StartGame();

        Console.WriteLine("Нажмите любую клавишу для выхода.");
        Console.ReadKey();
    }
}
