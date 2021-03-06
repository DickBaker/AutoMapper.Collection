using System;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace AutoMapper.EntityFramework
{
    public class Persistence<TTo> : IPersistence
        where TTo : class
    {
        private readonly DbSet<TTo> _sourceSet;
        private readonly IMapper _mapper;

        public Persistence(DbSet<TTo> sourceSet, IMapper mapper)
        {
            _sourceSet = sourceSet;
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public void InsertOrUpdate<TFrom>(TFrom from)
            where TFrom : class => InsertOrUpdate(typeof(TFrom), from);

        public void InsertOrUpdate(Type type, object from)
        {
            if (!(_mapper.Map(from, type, typeof(Expression<Func<TTo, bool>>)) is Expression<Func<TTo, bool>> equivExpr))
                return;

            var to = _sourceSet.FirstOrDefault(equivExpr);

            if (to == null)
            {
                to = _sourceSet.Create<TTo>();
                _sourceSet.Add(to);
            }
            _mapper.Map(from, to);
        }

        public void Remove<TFrom>(TFrom from)
            where TFrom : class
        {
            var equivExpr = _mapper.Map<TFrom, Expression<Func<TTo, bool>>>(from);
            if (equivExpr == null)
                return;
            var to = _sourceSet.FirstOrDefault(equivExpr);

            if (to != null)
                _sourceSet.Remove(to);
        }
    }
}