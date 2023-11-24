using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EFBugRepro
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var context = new TestContext();
            context.Database.EnsureCreated();
            var list = new MyClass[]
            {
                new MyClass(),
                new MyClass()
                {
                    MyClassId = 2
                }
            };

            var result = context.TestEntities.Join(list,
                x => x.DateTime,
                x => x.DateTime,
                (x, y) => new { x, y })
                .ToList();

            //expecting something like
            // SELECT *
            // FROM TestEntities
            // JOIN OPENJSON('[{"MyClassId": 1, "DateTime": "0001-01-01"}, {"MyClassId": 2, "DateTime": "0001-01-01"}]') WITH (
            // MyClassId bigint,
            // DateTime datetime2
            // ) AS c
            // ON TestEntities.Id = c.MyClassId;
        }
    }

    public class MyClass
    {
        public DateTime DateTime { get; set; }
        public int MyClassId { get; set; }
    }
    public class TestContext : DbContext
    {

        public TestContext() : base(new DbContextOptionsBuilder()
            .UseSqlServer("Server=localhost;Initial Catalog=test;User ID=sa;Password=password;TrustServerCertificate=True"
                ).Options)
        {
        }

        public DbSet<TestEntity> TestEntities { get; set; }
    }

    public class TestEntity
    {


        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public DateTime DateTime { get; set; }
    }
}
