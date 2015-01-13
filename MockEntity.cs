using System;
using System.Data.Entity;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Moq;
using System.Data.SqlClient;
//using namespace.of.Entity

namespace loctool.Mocks
{
    public class MockEntity<EntityInterface> where EntityInterface : class, IEntity
    {
        public Mock<EntityInterface> Entity { get; private set; }
        public Mock<ISqlConnection> SqlConnection { get; private set; }

        #region Constructor
        public MockEntity()
            : this(MockBehavior.Strict)
        {
        }
        public MockEntity(MockBehavior behavior)
        {
            this.Entity = new Mock<EntityInterface>(behavior);
            this.SqlConnection = new Mock<ISqlConnection>(behavior);

            this.Entity.Setup(s => s.GetConnection(It.IsAny<int>())).Returns(this.SqlConnection.Object);
            this.Entity.Setup(s => s.Dispose());
        }

        public InMemoryDbSet<TEntity> SetupDbSet<TEntity>(Expression<Func<EntityInterface, IDbSet<TEntity>>> expression, IEnumerable<TEntity> set = null) where TEntity : class
        {
            InMemoryDbSet<TEntity> dbSet = set == null ? new InMemoryDbSet<TEntity>() : new InMemoryDbSet<TEntity>(set);

            this.Entity.Setup(expression).Returns(dbSet);

            return dbSet;
        }
        #endregion

        #region Mock DbSet
        public class InMemoryDbSet<TEntity> : IDbSet<TEntity> where TEntity : class
        {
            readonly HashSet<TEntity> _set;
            readonly IQueryable<TEntity> _queryableSet;

            internal InMemoryDbSet()
                : this(Enumerable.Empty<TEntity>())
            {
            }

            internal InMemoryDbSet(IEnumerable<TEntity> entities)
            {
                _set = new HashSet<TEntity>();
                foreach (var entity in entities)
                {
                    _set.Add(entity);
                }
                _queryableSet = _set.AsQueryable();
            }

            public TEntity Add(TEntity entity)
            {
                _set.Add(entity);
                return entity;
            }

            public TEntity Attach(TEntity entity)
            {
                _set.Add(entity);
                return entity;
            }

            public TEntity Remove(TEntity entity)
            {
                _set.Remove(entity);
                return entity;
            }

            public virtual TDerivedEntity Create<TDerivedEntity>() where TDerivedEntity : class, TEntity
            {
                throw new NotImplementedException();
            }

            public virtual TEntity Create()
            {
                throw new NotImplementedException();
            }

            public virtual TEntity Find(params object[] keyValues)
            {
                return _set.FirstOrDefault();
            }

            public ObservableCollection<TEntity> Local
            {
                get { return new ObservableCollection<TEntity>(_queryableSet); }
            }

            public IEnumerator<TEntity> GetEnumerator()
            {
                return _queryableSet.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _queryableSet.GetEnumerator();
            }

            public Type ElementType
            {
                get { return _queryableSet.ElementType; }
            }

            public Expression Expression
            {
                get { return _queryableSet.Expression; }
            }

            public IQueryProvider Provider
            {
                get { return _queryableSet.Provider; }
            }
        }
        #endregion

        #region Create Sql Exception
        public static SqlException CreateSqlException(
            string Message,
            Exception innerexception,
            Guid conId,
            SqlError[] errors)
        {
            var errorCollection = CreateSqlErrorCollection(errors);
            Type[] types = new Type[] { typeof(string), typeof(SqlErrorCollection), typeof(Exception), typeof(Guid) };
            var ctor = typeof(SqlException).GetConstructor(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance, null, types, null);
            var value = ctor.Invoke
                (
                    new object[]
                    {
                        Message,
                        errorCollection,
                        innerexception,
                        conId
                    }
                ) as SqlException;
            return value;
        }

        public static SqlException CreateSqlException(
            string Message,
            Exception innerexception,
            Guid conId,
            SqlErrorCollection ErrorCollection
        )
        {
            Type[] types = new Type[] { typeof(string), typeof(SqlErrorCollection), typeof(Exception), typeof(Guid) };
            var ctor = typeof(SqlException).GetConstructor(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance, null, types, null);
            var value = ctor.Invoke
                (
                    new object[]
                    {
                        Message,
                        ErrorCollection,
                        innerexception,
                        conId
                    }
                ) as SqlException;
            return value;
        }

        public static SqlErrorCollection CreateSqlErrorCollection(
            params SqlError[] SqlErrors
        )
        {
            var ctor = typeof(SqlErrorCollection).GetConstructor(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                    null,
                    new System.Type[] { },
                    null
                );
            SqlErrorCollection result = ctor.Invoke
                (
                    new System.Type[] { }
                ) as SqlErrorCollection;

            var add = typeof(SqlErrorCollection).GetMethod
            (
                "Add",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance
            );
            foreach (SqlError err in SqlErrors)
            {
                add.Invoke(result, new object[] { err });
            }

            return (result);
        }

        public static SqlError CreateSqlError(
            int InfoNumber,
            byte ErrorState,
            byte ErrorClass,
            string Server,
            string ErrorMessage,
            string Procedure,
            int LineNumber
        )
        {
            var ctor = typeof(SqlError).GetConstructor
                (
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance,
                    null,
                    new System.Type[] 
                    { 
                        typeof(int), 
                        typeof(byte), 
                        typeof(byte), 
                        typeof(string), 
                        typeof(string),
                        typeof(string), 
                        typeof(int) 
                    },
                    null
                );
            var value = ctor.Invoke
                (
                    new object[]
                    {
                        InfoNumber,
                        ErrorState,
                        ErrorClass,
                        Server,
                        ErrorMessage,
                        Procedure,
                        LineNumber
                    }
                ) as SqlError;
            return value;
        }
        #endregion
    }
}
