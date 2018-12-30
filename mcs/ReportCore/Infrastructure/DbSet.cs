using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mono.Infrastructure
{
    // http://stackoverflow.com/questions/7431756/wrapping-dbsettentity-with-a-custom-dbset-idbset

    public interface IDbSet : IEnumerator<object>
    {
        bool Changed { get; set; }
        void SaveChanges();
    }

    public interface IDbSet<T> : ICollection<T> where T : class
    { 
    }
    
    //public interface IDbRecord

    public interface IIDEntity
    {
        int ID { get; set; }
    }


    public abstract class DbSet : IDbSet
    {
        public virtual bool Changed { get; set; }
        public abstract void SaveChanges();

        public abstract object Current { get; }
        public abstract bool MoveNext();
        public abstract void Reset();
        public abstract void Dispose();
    }

    // public virtual DbSet<T> ListT { get; set; }

    public abstract class DbSet<T> : DbSet, IDbSet<T> , ICollection<T>
        where T : class, new()
    {
        public abstract IEnumerator<T> GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
        public abstract int Count { get; }
        public abstract bool IsReadOnly { get; }

        public abstract void Add(T item);
        public abstract void Clear();
        public abstract bool Contains(T item);
        public abstract void CopyTo(T[] array, int arrayIndex);
        public abstract bool Remove(T item);

        public abstract IList<T> ToList();
    }

    public class DropCreateDatabaseIfModelChanges
    { }

    // System.Data.Entity.
    public class DropCreateDatabaseIfModelChanges<Context> : DropCreateDatabaseIfModelChanges where Context : class, new()
    {
        protected virtual void Seed(DbContext context) { }
    }

}


/*
//http://stackoverflow.com/questions/7431756/wrapping-dbsettentity-with-a-custom-dbset-idbset
//Context:
 * 
 *  cc.Contacts.Add(contact);
  cc.SaveChanges();


public class MyEfDataContext : DbContext
{
    public MyEfDataContext(string connectionString)
        : base(connectionString)
    {
        Database.SetInitializer<MyEfDataContext>(null);
    }

    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    {
        modelBuilder.Configurations.Add(new User.Configuration());
        modelBuilder.Configurations.Add(new Account.Configuration());
        base.OnModelCreating(modelBuilder);
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Account> Accounts { get; set; }
}
 
 Main(string[] args) 
 {
        IEntityFactory factory = new ImplEntityFactory("SQLConnectionString");
        IUser user = factory.Users.Find(5);
        IAccount usersAccount = user.Account;

        IAccount account = factory.Accounts.Find(3);
        Console.Write(account.Users.Count());
 }
*/