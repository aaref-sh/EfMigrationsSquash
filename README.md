# Squash your ef core migrations
if you tired of your .net project being building slower and slower? check your migrations, your `.Designer.cs` files contains huge amount of C# code (~10k lines for each migration) what make it takes really long time to get optimised/built

## How does it works?
It elemenates your .Designer.cs files away by moving the `Up` method content from migration `n` to the begining to the `Up` method of the migration `n+1`
and the content of `Down` method of migration `n` to the end of `Down` method of the migration `n+1`

## Does it have any side effects?
I'm not really sure, but i tried it on fresh database and existing databases as well and worked fine



## Why to use it?
 - speed up building: eleminating several `.Designer.cs` files will speed up your code analyze, parse and build
 - reduce the amount of old files.

## Why Not to use it?
 - if you have several live databases with a different version for each.
 - if you're planning to rollback your db to a specific migration (this is not possible, you'd just squashed them, duh!)

## How to use it:
 - Download it using `dotnet tool install --global EfMigrationsSquash` if you don't have it already 
 - cd to your migration project (or a project where do your migrations live)
 - run in terminal (EfMigrationsSquash), chose the start and end migration files to squash between
 - check the result

MAKE SURE TO NOT SQUASH BOTH APPLIED AND NOT APPLIED MIGRATIONS TOGETHER

This is a preview version, it works with .net8+
feel free to fork, contribute
