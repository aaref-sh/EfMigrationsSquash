## Squash your ef core migrations

# Why to use it?
 - speed up building: eliminating several .Design.cs files will speed up your code analyze, parse and build
 - reduce the amount of old files.

# Why Not to use it?
 - if you have several live databases with a different version for each.
 - if you're planning to rollback your db to a specific migration (this is not possible, you just squashed them, duh!)

# How to use it:
 - cd to your migration project (or a project where do your migrations live)
 - run in terminal (EfMigrationsSquash), chose the start and end migration files to squash between
 - check the result

MAKE SURE TO NOT SQUASH BOTH APPLIED AND NOT APPLIED MIGRATIONS TOGETHER

This is a preview version, it works with .net8+
