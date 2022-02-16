﻿#region License
// Copyright (c) 2021, FluentMigrator Project
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using FluentMigrator.Builders.Create.Index;
using FluentMigrator.Infrastructure;
using FluentMigrator.Infrastructure.Extensions;
using FluentMigrator.Postgres;

using JetBrains.Annotations;

namespace FluentMigrator.Builder.Create.Index
{
    public class CreateBTreeIndexOptionsSyntax : AbstractCreateIndexMethodOptionsSyntax, ICreateBTreeIndexOptionsSyntax
    {
        /// <inheritdoc />
        public CreateBTreeIndexOptionsSyntax([NotNull] ICreateIndexOptionsSyntax createIndexOptionsSyntax)
            : base(createIndexOptionsSyntax)
        {
        }

        /// <inheritdoc />
        public new ICreateBTreeIndexOptionsSyntax Fillfactor(int fillfactor)
        {
            base.Fillfactor(fillfactor);
            return this;
        }

        /// <inheritdoc />
        public ICreateBTreeIndexOptionsSyntax VacuumCleanupIndexScaleFactor(float point)
        {
            var additionalFeatures = CreateIndexOptionsSyntax as ISupportAdditionalFeatures;
            additionalFeatures.SetAdditionalFeature(PostgresExtensions.IndexVacuumCleanupIndexScaleFactor, point);
            return this;
        }
    }
}
