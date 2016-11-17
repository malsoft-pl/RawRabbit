﻿using System;
using System.Threading.Tasks;
using RawRabbit.Configuration.Consume;

namespace RawRabbit.Pipe.Middleware
{
	public class GetConfigurationOptions
	{
		public Func<IPipeContext, GetConfiguration> CreateFunc { get; set; }
		public Func<IPipeContext, Action<IGetConfigurationBuilder>> ConfigBuilderFunc { get; set; }
		public Action<IPipeContext, GetConfiguration> PostExecuteAction { get; set; }
	}

	public class GetConfigurationMiddleware : Middleware
	{
		protected Func<IPipeContext, GetConfiguration> CreateFunc;
		protected Action<IPipeContext, GetConfiguration> PostExecutionAction;
		protected Func<IPipeContext, Action<IGetConfigurationBuilder>> ConfigBuilderFunc;

		public GetConfigurationMiddleware(GetConfigurationOptions options = null)
		{
			CreateFunc = options?.CreateFunc ?? (context => new GetConfiguration());
			PostExecutionAction = options?.PostExecuteAction;
			ConfigBuilderFunc = options?.ConfigBuilderFunc ?? (context => context.Get<Action<IGetConfigurationBuilder>>(PipeKey.ConfigurationAction));
		}
		public override Task InvokeAsync(IPipeContext context)
		{
			var defaultCfg = CreateConfiguration(context);
			var configAction = GetConfigurationAction(context);
			var config = GetConfiguredConfiguration(defaultCfg, configAction);
			PostExecutionAction?.Invoke(context, config);
			context.Properties.TryAdd(PipeKey.GetConfiguration, config);
			return Next.InvokeAsync(context);
		}

		protected virtual GetConfiguration CreateConfiguration(IPipeContext context)
		{
			return CreateFunc(context);
		}

		protected virtual Action<IGetConfigurationBuilder> GetConfigurationAction(IPipeContext context)
		{
			return ConfigBuilderFunc(context);
		}

		protected virtual GetConfiguration GetConfiguredConfiguration(GetConfiguration configuration, Action<IGetConfigurationBuilder> action)
		{
			if (action == null)
			{
				return configuration;
			}
			var builder = new GetConfigurationBuilder(configuration);
			action.Invoke(builder);
			return builder.Configuration;
		}
	}
}
