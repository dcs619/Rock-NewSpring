/* BlockType attribute names */
SELECT [bt].[Name] as [BlockTypeName]
      ,[a].[Key]
      ,[a].[Name]
      ,[a].[Order]
      ,[ft].[Name] as [FieldType]
      ,[a].[Description]
      ,[a].[Id]
FROM [Attribute] [a]
join [EntityType] [e] on [e].[Id] = [a].[EntityTypeId]
join [BlockType] [bt] on [bt].[Id] = [a].[EntityTypeQualifierValue]
join [FieldType] [ft] on [ft].[Id] = [a].[FieldTypeId]
where [e].[Name] = 'Rock.Model.Block'
--order by [BlockTypeName], [Order]
order by a.id



