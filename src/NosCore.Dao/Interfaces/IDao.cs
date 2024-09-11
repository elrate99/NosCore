﻿//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace NosCore.Dao.Interfaces
{
    public interface IDao<out TDto>
    {
        IEnumerable<TDto> LoadAll();
    }

    public interface IDao<TDto, in TPk> : IDao<TDto>
    {
        Task<TDto> TryDeleteAsync(TPk dtokey);

        public Task<IEnumerable<TDto>?> TryDeleteAsync(IEnumerable<TPk> dtokeys);

        Task<TDto> FirstOrDefaultAsync(Expression<Func<TDto, bool>> predicate);

        Task<TDto> TryInsertOrUpdateAsync(TDto dto);

        Task<bool> TryInsertOrUpdateAsync(IEnumerable<TDto> dtos);

        IEnumerable<TDto>? Where(Expression<Func<TDto, bool>> predicate);
    }
}